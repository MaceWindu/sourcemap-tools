using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.SourcemapParser;

/// <summary>
/// Source map object.
/// </summary>
// :-/ unsealed just for test mocks to work
public class SourceMap
{
	/// <summary>
	///  Cache the comparer to save the allocation.
	/// </summary>
	[JsonIgnore]
	private static readonly Comparer<MappingEntry> _comparer = Comparer<MappingEntry>.Create((a, b) => a.GeneratedSourcePosition.CompareTo(b.GeneratedSourcePosition));

	/// <summary>
	/// The version of the source map specification being used.
	/// </summary>
	[JsonPropertyName("version")]
	public int Version { get; set; }

	/// <summary>
	/// The name of the generated file to which this source map corresponds.
	/// </summary>
	[JsonPropertyName("file")]
	public string? File { get; set; }

	/// <summary>
	/// The raw, unparsed mappings entry of the source map.
	/// </summary>
	[JsonPropertyName("mappings")]
	public string? Mappings { get; set; }

	/// <summary>
	/// The list of source files that were the inputs used to generate this output file.
	/// </summary>
	[JsonPropertyName("sources")]
	public IReadOnlyList<string>? Sources { get; set; }

	/// <summary>
	/// A list of known original names for entries in this file.
	/// </summary>
	[JsonPropertyName("names")]
	public IReadOnlyList<string>? Names { get; set; }

	/// <summary>
	/// Parsed version of the mappings string that is used for getting original names and source positions.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<MappingEntry>? ParsedMappings { get; set; }

	/// <summary>
	/// A list of content source files.
	/// </summary>
	public IReadOnlyList<string>? SourcesContent { get; set; }

	/// <summary>
	/// Json deserialization constructor.
	/// </summary>
	public SourceMap()
	{
	}

	/// <summary>
	/// Creates new instance of source map object.
	/// See <a href="https://sourcemaps.info/spec.html"></a> for more details.
	/// </summary>
	public SourceMap(
		int version,
		string? file,
		string? mappings,
		IReadOnlyList<string>? sources,
		IReadOnlyList<string>? names,
		IReadOnlyList<MappingEntry> parsedMappings,
		IReadOnlyList<string>? sourcesContent)
	{
		Version = version;
		File = file;
		Mappings = mappings;
		Sources = sources;
		Names = names;
		ParsedMappings = parsedMappings;
		SourcesContent = sourcesContent;
	}

	/// <summary>
	/// Creates copy fo source map.
	/// </summary>
	/// <returns>Returns copy of current source map object.</returns>
	public SourceMap Clone() => new(Version, File, Mappings, Sources, Names, ParsedMappings ?? new List<MappingEntry>(), SourcesContent);

	/// <summary>
	/// Applies the mappings of a sub source map to the current source map
	/// Each mapping to the supplied source file is rewritten using the supplied source map
	/// This is useful in situations where we have a to b to c, with mappings ba.map and cb.map
	/// Calling cb.ApplySourceMap(ba) will return mappings from c to a (ca).
	/// <param name="submap">The submap to apply.</param>
	/// <param name="sourceFile">The filename of the source file. If not specified, submap's File property will be used.</param>
	/// <returns>A new source map.</returns>
	/// </summary>
	public SourceMap ApplySourceMap(SourceMap submap, string? sourceFile = null)
	{
		if (submap == null)
		{
			throw new ArgumentNullException(nameof(submap));
		}

		if (sourceFile == null)
		{
			if (submap.File == null)
			{
				throw new InvalidOperationException($"{nameof(ApplySourceMap)} expects either the explicit source file to the map, or submap's 'file' property");
			}

			sourceFile = submap.File;
		}

		var sources = new HashSet<string>(StringComparer.Ordinal);
		var names = new HashSet<string>(StringComparer.Ordinal);
		IReadOnlyList<MappingEntry>? parsedMappings = null;

		if (ParsedMappings != null && ParsedMappings.Count > 0)
		{
			var idx = 0;
			var parsedMappingsArray = new MappingEntry[ParsedMappings.Count];

			// transform mappings in this source map
			foreach (var mappingEntry in ParsedMappings)
			{
				var newMappingEntry = mappingEntry;

				if (mappingEntry.OriginalFileName == sourceFile && mappingEntry.OriginalSourcePosition != SourcePosition.NotFound)
				{
					var correspondingSubMapMappingEntry = submap.GetMappingEntryForGeneratedSourcePosition(mappingEntry.OriginalSourcePosition);

					if (correspondingSubMapMappingEntry != null)
					{
						// Copy the mapping
						newMappingEntry = new MappingEntry(
							mappingEntry.GeneratedSourcePosition,
							correspondingSubMapMappingEntry.Value.OriginalSourcePosition,
							correspondingSubMapMappingEntry.Value.OriginalName ?? mappingEntry.OriginalName,
							correspondingSubMapMappingEntry.Value.OriginalFileName ?? mappingEntry.OriginalFileName);
					}
				}

				// Copy into "Sources" and "Names"
				var originalFileName = newMappingEntry.OriginalFileName;
				var originalName = newMappingEntry.OriginalName;

				if (originalFileName != null)
				{
					sources.Add(originalFileName);
				}

				if (originalName != null)
				{
					names.Add(originalName);
				}

				parsedMappingsArray[idx] = newMappingEntry;
				idx++;
			}

			parsedMappings = parsedMappingsArray;
		}

		return new SourceMap(
			Version,
			File,
			null,
			sources.ToList(),
			names.ToList(),
			parsedMappings ?? Array.Empty<MappingEntry>(),
			new List<string>());
	}

	/// <summary>
	/// Finds the mapping entry for the generated source position. If no exact match is found, it will attempt
	/// to return a nearby mapping that should map to the same piece of code.
	/// </summary>
	/// <param name="generatedSourcePosition">The location in generated code for which we want to discover a mapping entry.</param>
	/// <returns>A mapping entry that is a close match for the desired generated code location.</returns>
	// :-/ virtual just for test mocks to work
	public virtual MappingEntry? GetMappingEntryForGeneratedSourcePosition(SourcePosition generatedSourcePosition)
	{
		if (ParsedMappings == null)
		{
			return null;
		}

		var mappingEntryToFind = new MappingEntry(generatedSourcePosition);

		var index = ParsedMappings.BinarySearch(mappingEntryToFind, _comparer);

		// If we didn't get an exact match, let's try to return the closest piece of code to the given line
		if (index < 0)
		{
			// The BinarySearch method returns the bitwise complement of the nearest element that is larger than the desired element when there isn't a match.
			// Based on tests with source maps generated with the Closure Compiler, we should consider the closest source position that is smaller than the target value when we don't have a match.
			var correctIndex = ~index - 1;

			if (correctIndex >= 0 && ParsedMappings[correctIndex].GeneratedSourcePosition.IsEqualish(generatedSourcePosition))
			{
				index = correctIndex;
			}
		}

		return index >= 0 ? ParsedMappings[index] : null;
	}
}
