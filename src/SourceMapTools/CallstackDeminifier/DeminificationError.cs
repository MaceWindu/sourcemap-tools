namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Enum indicating if there were any errors encountered when attempting to deminify the StakFrame.
	/// </summary>
	public enum DeminificationError
	{
		/// <summary>
		/// No error was encountered durring deminification of the StackFrame.
		/// </summary>
		None,

		/// <summary>
		/// There was no source code provided by the ISourceCodeProvider.
		/// </summary>
		NoSourceCodeProvided,

		/// <summary>
		/// The function that wraps the minified stack frame could not be determined.
		/// </summary>
		NoWrapingFunctionFound,

		/// <summary>
		/// There was not a valid source map returned by ISourceMapProvider.GetSourceMapForUrl.
		/// </summary>
		NoSourceMap,

		/// <summary>
		/// A mapping entry was not found for the source position of the minified stack frame.
		/// </summary>
		NoMatchingMapingInSourceMap,
	}
}
