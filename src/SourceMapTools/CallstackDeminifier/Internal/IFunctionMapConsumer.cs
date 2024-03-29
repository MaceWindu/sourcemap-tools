﻿using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Internal API.
/// </summary>
public interface IFunctionMapConsumer
{
	/// <summary>
	/// Finds the JavaScript function that wraps the given source location.
	/// </summary>
	/// <param name="sourcePosition">The location of the code around which we wish to find a wrapping function.</param>
	/// <param name="functionMap">The function map, sorted in decreasing order by start source position, that represents the file containing the code of interest.</param>
	FunctionMapEntry? GetWrappingFunctionForSourceLocation(SourcePosition sourcePosition, IReadOnlyList<FunctionMapEntry> functionMap);
}