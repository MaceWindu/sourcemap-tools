using System.Collections.Generic;
using System.IO;
using Esprima;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	internal class FunctionMapGenerator : IFunctionMapGenerator
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
			catch
			{
				// Failed to parse JavaScript source. This is common as the JS parser does not support ES2015+.
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

			// Sort in descending order by start position.  This allows the first result found in a linear search to be the "closest function to the [consumer's] source position".
			//
			// ATTN: It may be possible to do this with an ascending order sort, followed by a series of binary searches on rows & columns.
			//       Our current profiles show the memory pressure being a bigger issue than the stack lookup, so I'm leaving this for now.
			functionFinderVisitor.FunctionMap.Sort((x, y) => y.Start.CompareTo(x.Start));

			return functionFinderVisitor.FunctionMap;
		}
	}
}
