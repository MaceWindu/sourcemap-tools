using System;
using System.Collections.Generic;

namespace SourcemapToolkit.CallstackDeminifier
{
	public class DeminifyStackTraceResult
	{
		internal DeminifyStackTraceResult(
			IReadOnlyList<StackFrame> minifiedStackFrames,
			IReadOnlyList<StackFrameDeminificationResult> deminifiedStackFrameResults,
			string? message)
		{
			MinifiedStackFrames = minifiedStackFrames;
			DeminifiedStackFrameResults = deminifiedStackFrameResults;
			Message = message;
		}

		public string? Message { get; }

		public IReadOnlyList<StackFrame> MinifiedStackFrames { get; }

		public IReadOnlyList<StackFrameDeminificationResult> DeminifiedStackFrameResults { get; }

		public override string ToString()
		{
			var output = Message ?? string.Empty;
			for (var i = 0; i < DeminifiedStackFrameResults.Count; i++)
			{
				var deminFrame = DeminifiedStackFrameResults[i].DeminifiedStackFrame;

				// Use deminified info wherever possible, merging if necessary so we always print a full frame
				var frame = new StackFrame(
					deminFrame.MethodName ?? MinifiedStackFrames[i].MethodName,
					deminFrame.SourcePosition != null ? deminFrame.FilePath : MinifiedStackFrames[i].FilePath,
					deminFrame.SourcePosition ?? MinifiedStackFrames[i].SourcePosition);

				output += $"{Environment.NewLine}  {frame}";
			}

			return output;
		}
	}
}
