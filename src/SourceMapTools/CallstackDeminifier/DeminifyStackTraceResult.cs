using System.Collections.Generic;
using System.Text;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier;

/// <summary>
/// Contains stack trace details (both minified and deminified).
/// </summary>
/// <param name="Message">Gets error message, associated with stack trace.</param>
/// <param name="MinifiedStackFrames">Gets list of stack frames for minified stack.</param>
/// <param name="DeminifiedStackFrameResults">Gets list of stack frames for de-minified stack.</param>
public sealed record DeminifyStackTraceResult(
		string? Message,
		IReadOnlyList<StackFrame> MinifiedStackFrames,
		IReadOnlyList<StackFrameDeminificationResult> DeminifiedStackFrameResults)
{
	/// <summary>
	/// Returns string that represents stack trace.
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
			var deminifiedFrame = DeminifiedStackFrameResults[i].DeminifiedStackFrame;

			// Use deminified info wherever possible, merging if necessary so we always print a full frame
			var frame = new StackFrame(
				deminifiedFrame.MethodName ?? MinifiedStackFrames[i].MethodName,
				deminifiedFrame.SourcePosition != SourcePosition.NotFound ? deminifiedFrame.FilePath : MinifiedStackFrames[i].FilePath,
				deminifiedFrame.SourcePosition != SourcePosition.NotFound ? deminifiedFrame.SourcePosition : MinifiedStackFrames[i].SourcePosition);

			sb
				.AppendLine()
				.Append("  ")
				.Append(frame);
		}

		return sb.ToString();
	}
}
