﻿using System;
using System.Collections.Generic;

namespace SourcemapToolkit.SourcemapParser;

/// <summary>
/// Helper to compress source map by removing entries, mapped to same line.
/// </summary>
public static class SourceMapTransformer
{
	/// <summary>
	/// Removes column information from a source map
	/// This can significantly reduce the size of source maps
	/// If there is a tie between mapping entries, the first generated line takes priority.
	/// <returns>A new source map.</returns>
	/// </summary>
	public static SourceMap Flatten(SourceMap sourceMap)
	{
		if (sourceMap == null)
		{
			throw new ArgumentNullException(nameof(sourceMap));
		}

		IReadOnlyList<MappingEntry>? mappingEntries = null;

		if (sourceMap.ParsedMappings != null && sourceMap.ParsedMappings.Count > 0)
		{
			var visitedLines = new HashSet<int>();
			var parsedMappings = new List<MappingEntry>(sourceMap.ParsedMappings.Count); // assume each line will not have been visited before

			foreach (var mapping in sourceMap.ParsedMappings)
			{
				var generatedLine = mapping.GeneratedSourcePosition.Line;

				if (visitedLines.Add(generatedLine))
				{
					var newMapping = mapping.CloneWithResetColumnNumber();
					parsedMappings.Add(newMapping);
				}
			}

			// Free-up any unneeded space.  This no-ops if we're already the right size.
			parsedMappings.Capacity = parsedMappings.Count;
			mappingEntries = parsedMappings;
		}

		return new SourceMap(
			sourceMap.Version,
			sourceMap.File,
			sourceMap.Mappings,
			sourceMap.Sources,
			sourceMap.Names,
			mappingEntries ?? [],
			sourceMap.SourcesContent);
	}
}
