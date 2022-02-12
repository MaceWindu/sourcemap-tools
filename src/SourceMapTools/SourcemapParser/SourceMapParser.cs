﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Source map parser.
	/// </summary>
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
				var result = JsonSerializer.Deserialize<SourceMap>(sourceMapStream);
				if (result != null)
				{
					// Since SourceMap is immutable we need to allocate a new one and copy over all the information
					var parsedMappings = MappingsListParser.ParseMappings(result.Mappings ?? string.Empty, result.Names ?? new List<string>(), result.Sources ?? new List<string>());

					result = new SourceMap(
						result.Version,
						result.File,
						result.Mappings,
						result.Sources,
						result.Names,
						parsedMappings,
						result.SourcesContent);
				}

				return result;
			}
		}
	}
}
