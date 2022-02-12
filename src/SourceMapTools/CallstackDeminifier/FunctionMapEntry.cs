using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Contains information regarding the location of a particular function in a JavaScript file
	/// </summary>
	internal sealed class FunctionMapEntry
	{
		public FunctionMapEntry(
			IReadOnlyList<BindingInformation> bindings,
			string? deminifiedMethodName,
			SourcePosition start,
			SourcePosition end)
		{
			Bindings = bindings;
			DeminifiedMethodName = deminifiedMethodName;
			Start = start;
			End = end;
		}

		/// <summary>
		/// A list of bindings that are associated with this function map entry.
		/// To get the complete name of the function associated with this mapping entry
		/// append the names of each bindings with a "."
		/// </summary>
		public IReadOnlyList<BindingInformation> Bindings { get; }

		/// <summary>
		/// If this entry represents a function whose name was minified, this value 
		/// may contain an associated deminfied name corresponding to the function.
		/// </summary>
		public string? DeminifiedMethodName { get; }

		/// <summary>
		/// Denotes the location of the beginning of this function
		/// </summary>
		public SourcePosition Start { get; }

		/// <summary>
		/// Denotes the end location of this function
		/// </summary>
		public SourcePosition End { get; }
	}
}
