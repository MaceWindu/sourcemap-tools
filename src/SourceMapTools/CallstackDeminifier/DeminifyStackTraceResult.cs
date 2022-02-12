using System.Collections.Generic;
using System.Text;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Contains stack trace details (both minified and diminified).
	/// </summary>
	public sealed class DeminifyStackTraceResult
	{
		internal DeminifyStackTraceResult(
			string? message,
			IReadOnlyList<StackFrame> minifiedStackFrames,
			IReadOnlyList<StackFrameDeminificationResult> deminifiedStackFrameResults)
		{
			MinifiedStackFrames = minifiedStackFrames;
			DeminifiedStackFrameResults = deminifiedStackFrameResults;
			Message = message;
		}

		/// <summary>
		/// Gets error message, associated with stack trace.
		/// </summary>
		public string? Message { get; }

		/// <summary>
		/// Gets list of stack frames for minified stack.
		/// </summary>
		public IReadOnlyList<StackFrame> MinifiedStackFrames { get; }

		/// <summary>
		/// Gets list of stack frames for de-minified stack.
		/// </summary>
		public IReadOnlyList<StackFrameDeminificationResult> DeminifiedStackFrameResults { get; }

		/// <summary>
		/// Returns string that represents stack trace
		/// </summary>
		public override string ToString()
		{
			var sb = new StringBuilder();

			if (!string.IsNullOrEmpty(Message))
			{
				sb.Append(Message);
			}

			for (var i = 0; i < DeminifiedStackFrameResults.Count; i++)
			{
				var deminFrame = DeminifiedStackFrameResults[i].DeminifiedStackFrame;

				// Use deminified info wherever possible, merging if necessary so we always print a full frame
				var frame = new StackFrame(
					deminFrame.MethodName ?? MinifiedStackFrames[i].MethodName,
					deminFrame.SourcePosition != SourcePosition.NotFound ? deminFrame.FilePath : MinifiedStackFrames[i].FilePath,
					deminFrame.SourcePosition != SourcePosition.NotFound ? deminFrame.SourcePosition : MinifiedStackFrames[i].SourcePosition);

				sb
					.AppendLine()
					.Append("  ")
					.Append(frame);
			}

			return sb.ToString();
		}
	}
}
