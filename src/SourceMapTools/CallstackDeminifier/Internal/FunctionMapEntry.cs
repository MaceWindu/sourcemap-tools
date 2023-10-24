using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Contains information regarding the location of a particular function in a JavaScript file.
/// </summary>
/// <param name="Bindings">A list of bindings that are associated with this function map entry. To get the complete name of the function associated with this mapping entry append the names of each bindings with a ".".</param>
/// <param name="DeminifiedMethodName">If this entry represents a function whose name was minified, this value may contain an associated deminified name corresponding to the function.</param>
/// <param name="Start">Denotes the location of the beginning of this function.</param>
/// <param name="End">Denotes the end location of this function.</param>
public sealed record FunctionMapEntry(
	IReadOnlyList<BindingInformation> Bindings,
	string? DeminifiedMethodName,
	SourcePosition Start,
	SourcePosition End);
