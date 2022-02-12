using System.Collections.Generic;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// This class is responsible for parsing a callstack string into
	/// a list of StackFrame objects and providing the deminified version
	/// of the stack frame.
	/// </summary>
	public sealed class StackTraceDeminifier
	{
		private readonly IStackFrameDeminifier _stackFrameDeminifier;
		private readonly IStackTraceParser _stackTraceParser;

		internal StackTraceDeminifier(IStackFrameDeminifier stackFrameDeminifier, IStackTraceParser stackTraceParser)
		{
			_stackFrameDeminifier = stackFrameDeminifier;
			_stackTraceParser = stackTraceParser;
		}

		/// <summary>
		/// Parses and deminifies a string containing a minified stack trace.
		/// </summary>
		/// <param name="stackTraceString">stack trace as string, to deobfuscate</param>
		/// <param name="preferSourceMapsSymbols">if true, we will use exact sourcemap names for deobfuscation, without guessing the wrapper function name from source code</param>
		/// <returns>Stack trace deminification result.</returns>
		public DeminifyStackTraceResult DeminifyStackTrace(string stackTraceString, bool preferSourceMapsSymbols)
		{
			var minifiedFrames = _stackTraceParser.ParseStackTrace(stackTraceString, out var message);
			var deminifiedFrames = new List<StackFrameDeminificationResult>(minifiedFrames.Count);

			// Deminify frames in reverse order so we can pass the symbol name from caller
			// (i.e. the function name) into the next level's deminification.
			string? callerSymbolName = null;
			for (var i = minifiedFrames.Count - 1; i >= 0; i--)
			{
				var frame = _stackFrameDeminifier.DeminifyStackFrame(minifiedFrames[i], callerSymbolName, preferSourceMapsSymbols);
				callerSymbolName = frame.DeminifiedSymbolName;
				deminifiedFrames.Add(frame);
			}

			deminifiedFrames.Reverse();

			if (preferSourceMapsSymbols)
			{
				// we want to move all method names by one frame, so each frame will contain caller name and not callee name. To make callstacks more familiar to C# and js debug versions.
				// However, for first frame we want to keep calee name (if available) as well since this is interesting info we don't want to lose.
				// However it means that for last frame (N), if have more then 1 frame in callstack, N-1 frame will have the same name.
				// It is confusing, so lets replace last one with null. This will cause toString to use the obfuscated name
				for (var i = 0; i < deminifiedFrames.Count - 1; i++)
				{
					var parentMethodName = deminifiedFrames[i + 1].DeminifiedStackFrame.MethodName;
					if (i == 0 && deminifiedFrames[i].DeminifiedStackFrame.MethodName != null)
					{
						parentMethodName += parentMethodName == null ? "=> " : " => ";
						parentMethodName += deminifiedFrames[i].DeminifiedStackFrame.MethodName;
					}
					deminifiedFrames[i].DeminifiedStackFrame.MethodName = parentMethodName;
				}

				if (deminifiedFrames.Count > 1)
				{
					deminifiedFrames[^1].DeminifiedStackFrame.MethodName = null;
				}
			}

			return new DeminifyStackTraceResult(message, minifiedFrames, deminifiedFrames);
		}
	}
}
