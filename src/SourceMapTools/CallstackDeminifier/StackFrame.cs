using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Represents a single entry in a JavaScript stack frame. 
	/// </summary>
	public class StackFrame
	{
		public StackFrame(string? methodName)
			: this(methodName, null, null)
		{
		}

		public StackFrame(string? methodName, string? filePath, SourcePosition? sourcePosition)
		{
			MethodName = methodName;
			FilePath = filePath;
			SourcePosition = sourcePosition;
		}

		/// <summary>
		/// The name of the method
		/// </summary>
		public string? MethodName { get; }

		/// <summary>
		/// The path of the file where this code is defined
		/// </summary>
		public string? FilePath { get; internal set; }

		/// <summary>
		/// The zero-based position of this stack entry.
		/// </summary>
		public SourcePosition? SourcePosition { get; internal set; }

		public override string ToString()
		{
			var output = $"at {(string.IsNullOrWhiteSpace(MethodName) ? "?" : MethodName)}";
			if (!string.IsNullOrWhiteSpace(FilePath))
			{
				output += $" in {FilePath}";
				if (SourcePosition != null)
				{
					output += $":{SourcePosition.Line + 1}:{SourcePosition.Column + 1}";
				}
			}
			return output;
		}
	}
}
