using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// Corresponds to a single parsed entry in the source map mapping string that is used internally by the parser.
/// The public API exposes the MappingEntry object, which is more useful to consumers of the library.
/// </summary>
/// <param name="GeneratedLineNumber">The zero-based line number in the generated code that corresponds to this mapping segment.</param>
/// <param name="GeneratedColumnNumber">The zero-based column number in the generated code that corresponds to this mapping segment.</param>
/// <param name="OriginalSourceFileIndex">The zero-based index into the sources array that corresponds to this mapping segment.</param>
/// <param name="OriginalLineNumber">The zero-based line number in the source code that corresponds to this mapping segment.</param>
/// <param name="OriginalColumnNumber">The zero-based line number in the source code that corresponds to this mapping segment.</param>
/// <param name="OriginalNameIndex">The zero-based index into the names array that can be used to identify names associated with this object.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct NumericMappingEntry(
	int GeneratedLineNumber,
	int GeneratedColumnNumber,
	int? OriginalSourceFileIndex,
	int? OriginalLineNumber,
	int? OriginalColumnNumber,
	int? OriginalNameIndex)
{
	/// <summary>Internal API.</summary>
	public MappingEntry ToMappingEntry(IReadOnlyList<string> names, IReadOnlyList<string> sources)
	{
		if (names is null)
		{
			throw new ArgumentNullException(nameof(names));
		}
		if (sources is null)
		{
			throw new ArgumentNullException(nameof(sources));
		}

		var originalSourcePosition = OriginalColumnNumber is not null && OriginalLineNumber is not null ? new SourcePosition(OriginalLineNumber.Value, OriginalColumnNumber.Value)
			: SourcePosition.NotFound;

		string? originalName = null;
		if (OriginalNameIndex is not null)
		{
			if (OriginalNameIndex.Value < 0 || OriginalNameIndex.Value >= names.Count)
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Source map contains original name index (={OriginalNameIndex.Value}) that is outside the range of the provided names array[{names.Count}]"));
			}

			originalName = names[OriginalNameIndex.Value];
		}

		string? originalFileName = null;
		if (OriginalSourceFileIndex is not null)
		{
			if (OriginalSourceFileIndex.Value < 0 || OriginalSourceFileIndex.Value >= sources.Count)
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Source map contains original name index (={OriginalSourceFileIndex.Value}) that is outside the range of the provided names array[{sources.Count}]"));
			}

			originalFileName = sources[OriginalSourceFileIndex.Value];
		}

		return new MappingEntry(
			new SourcePosition(GeneratedLineNumber, GeneratedColumnNumber),
			originalSourcePosition,
			originalName,
			originalFileName);
	}
}
