using System.Collections.Generic;
using System.IO;
using Esprima;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	internal sealed class FunctionMapGenerator : IFunctionMapGenerator
	{
		/// <summary>
		/// Returns a FunctionMap describing the locations of every funciton in the source code.
		/// The functions are to be sorted descending by start position.
		/// </summary>
		IReadOnlyList<FunctionMapEntry>? IFunctionMapGenerator.GenerateFunctionMap(Stream? sourceCodeStream, SourceMap? sourceMap)
		{
			if (sourceCodeStream == null || sourceMap == null)
			{
				return null;
			}

			IReadOnlyList<FunctionMapEntry>? result;
			try
			{
				result = ParseSourceCode(sourceCodeStream, sourceMap);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch
#pragma warning restore CA1031 // Do not catch general exception types
			{
				// Failed to parse JavaScript source
				// Continue to regular source map deminification.
				result = null;
			}

			return result;
		}

		/// <summary>
		/// Iterates over all the code in the JavaScript file to get a list of all the functions declared in that file.
		/// </summary>
		internal static IReadOnlyList<FunctionMapEntry> ParseSourceCode(Stream sourceCodeStream, SourceMap sourceMap)
		{
			string sourceCode;
			using (sourceCodeStream)
			using (var sr = new StreamReader(sourceCodeStream))
			{
				sourceCode = sr.ReadToEnd();
			}

			var jsParser = new JavaScriptParser(sourceCode);

			var script = jsParser.ParseScript();

			var functionFinderVisitor = new FunctionFinderVisitor(sourceMap);
			functionFinderVisitor.Visit(script);

			return functionFinderVisitor.GetFunctionMap();
		}
	}
}
