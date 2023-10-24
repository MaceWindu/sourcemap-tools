using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier;

/// <summary>
/// Represents a single entry in a JavaScript stack frame.
/// </summary>
/// <remarks>
/// Creates new instance stack frame with specified method name and source position.
/// </remarks>
/// <param name="methodName">Name of method, corresponding to current stack frame.</param>
/// <param name="filePath">Path to file that contains current stack frame method.</param>
/// <param name="sourcePosition">Position of stack frame in source file.</param>
public sealed class StackFrame(string? methodName, string? filePath, SourcePosition sourcePosition)
{
	/// <summary>
	/// Creates new instance stack frame with specified method name.
	/// </summary>
	/// <param name="methodName">Name of method, corresponding to current stack frame.</param>
	public StackFrame(string? methodName)
		: this(methodName, null, SourcePosition.NotFound)
	{
	}

	/// <summary>
	/// The name of the method.
	/// </summary>
	public string? MethodName { get; set; } = methodName;

	/// <summary>
	/// The path of the file where this code is defined.
	/// </summary>
	public string? FilePath { get; set; } = filePath;

	/// <summary>
	/// The zero-based position of this stack entry.
	/// </summary>
	public SourcePosition SourcePosition { get; internal set; } = sourcePosition;

	/// <summary>
	/// Returns text representation of current stack frame.
	/// </summary>
	public override string ToString()
	{
		var output = $"at {(string.IsNullOrWhiteSpace(MethodName) ? "?" : MethodName)}";
		if (!string.IsNullOrWhiteSpace(FilePath))
		{
			output += $" in {FilePath}";
			if (SourcePosition != SourcePosition.NotFound)
			{
				output += $":{SourcePosition.Line + 1}:{SourcePosition.Column + 1}";
			}
		}
		return output;
	}
}
