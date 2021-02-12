namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Represents the result of attmpting to deminify a single entry in a JavaScript stack frame. 
	/// </summary>
	public class StackFrameDeminificationResult
	{
		internal StackFrameDeminificationResult(
			DeminificationError deminificationError,
			StackFrame deminifiedStackFrame)
		{
			DeminificationError = deminificationError;
			DeminifiedStackFrame = deminifiedStackFrame;
		}

		/// <summary>
		/// The deminified StackFrame.
		/// </summary>
		public StackFrame DeminifiedStackFrame { get; }

		/// <summary>
		/// The original name of the symbol at this frame's position
		/// </summary>
		public string? DeminifiedSymbolName { get; internal set; }

		/// <summary>
		/// An enum indicating if any errors occured when deminifying the stack frame.
		/// </summary>
		public DeminificationError DeminificationError { get; internal set; }
	}
}
