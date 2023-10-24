namespace SourcemapToolkit.CallstackDeminifier;

/// <summary>
/// Represents the result of attempting to deminify a single entry in a JavaScript stack frame.
/// </summary>
public sealed class StackFrameDeminificationResult(
	DeminificationError deminificationError,
	StackFrame deminifiedStackFrame)
{
	/// <summary>
	/// The deminified StackFrame.
	/// </summary>
	public StackFrame DeminifiedStackFrame { get; } = deminifiedStackFrame;

	/// <summary>
	/// The original name of the symbol at this frame's position.
	/// </summary>
	public string? DeminifiedSymbolName { get; internal set; }

	/// <summary>
	/// An enum indicating if any errors occurred when deminifying the stack frame.
	/// </summary>
	public DeminificationError DeminificationError { get; internal set; } = deminificationError;
}
