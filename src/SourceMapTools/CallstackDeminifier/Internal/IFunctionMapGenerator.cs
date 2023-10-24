using System.Collections.Generic;
using System.IO;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Internal API.
/// </summary>
public interface IFunctionMapGenerator
{
	/// <summary>
	/// Returns a FunctionMap describing the locations of every function in the source code.
	/// The functions are to be sorted in decreasing order by start position.
	/// </summary>
	IReadOnlyList<FunctionMapEntry>? GenerateFunctionMap(Stream? sourceCodeStream, SourceMap? sourceMap);
}