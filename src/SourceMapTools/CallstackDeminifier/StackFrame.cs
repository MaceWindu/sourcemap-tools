using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Represents a single entry in a JavaScript stack frame. 
	/// </summary>
	public class StackFrame
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
		/// Creates new instance stack frame with specified method name and source position.
		/// </summary>
		/// <param name="methodName">Name of method, corresponding to current stack frame.</param>
		/// <param name="filePath">Path to file that contains current stack frame method.</param>
		/// <param name="sourcePosition">Position of stack frame in source file.</param>
		public StackFrame(string? methodName, string? filePath, SourcePosition sourcePosition)
		{
			MethodName = methodName;
			FilePath = filePath;
			SourcePosition = sourcePosition;
		}

		/// <summary>
		/// The name of the method
		/// </summary>
		public string? MethodName { get; internal set; }

		/// <summary>
		/// The path of the file where this code is defined
		/// </summary>
		public string? FilePath { get; internal set; }

		/// <summary>
		/// The zero-based position of this stack entry.
		/// </summary>
		public SourcePosition SourcePosition { get; internal set; } = SourcePosition.NotFound;

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
}
