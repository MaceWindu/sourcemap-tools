using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SourcemapToolkit.SourcemapParser
{
	public static class SourceMapParser
	{
		/// <summary>
		/// Parses a stream representing a source map into a SourceMap object.
		/// </summary>
		public static SourceMap? ParseSourceMap(Stream? sourceMapStream)
		{
			if (sourceMapStream == null)
			{
				return null;
			}

			using (sourceMapStream)
			{
				// https://github.com/dotnet/runtime/issues/1574
				var result = JsonSerializer.DeserializeAsync<SourceMap>(sourceMapStream).AsTask().GetAwaiter().GetResult();
				if (result != null)
				{
					result.ParsedMappings = MappingsListParser.ParseMappings(result.Mappings ?? string.Empty, result.Names ?? new List<string>(), result.Sources ?? new List<string>());
				}

				return result;
			}
		}
	}
}
