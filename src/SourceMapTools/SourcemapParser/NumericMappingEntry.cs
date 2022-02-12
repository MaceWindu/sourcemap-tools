using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Corresponds to a single parsed entry in the source map mapping string that is used internally by the parser.
	/// The public API exposes the MappingEntry object, which is more useful to consumers of the library.
	/// </summary>
	[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not used in equality")]
	internal readonly struct NumericMappingEntry
	{
		public NumericMappingEntry(
			int generatedLineNumber,
			int generatedColumnNumber,
			int? originalSourceFileIndex,
			int? originalLineNumber,
			int? originalColumnNumber,
			int? originalNameIndex)
		{
			GeneratedLineNumber = generatedLineNumber;
			GeneratedColumnNumber = generatedColumnNumber;
			OriginalSourceFileIndex = originalSourceFileIndex;
			OriginalLineNumber = originalLineNumber;
			OriginalColumnNumber = originalColumnNumber;
			OriginalNameIndex = originalNameIndex;
		}

		/// <summary>
		/// The zero-based line number in the generated code that corresponds to this mapping segment.
		/// </summary>
		public int GeneratedLineNumber { get; }

		/// <summary>
		/// The zero-based column number in the generated code that corresponds to this mapping segment.
		/// </summary>
		public int GeneratedColumnNumber { get; }

		/// <summary>
		/// The zero-based index into the sources array that corresponds to this mapping segment.
		/// </summary>
		public int? OriginalSourceFileIndex { get; }

		/// <summary>
		/// The zero-based line number in the source code that corresponds to this mapping segment.
		/// </summary>
		public int? OriginalLineNumber { get; }

		/// <summary>
		/// The zero-based line number in the source code that corresponds to this mapping segment.
		/// </summary>
		public int? OriginalColumnNumber { get; }

		/// <summary>
		/// The zero-based index into the names array that can be used to identify names associated with this object.
		/// </summary>
		public int? OriginalNameIndex { get; }

		public MappingEntry ToMappingEntry(IReadOnlyList<string> names, IReadOnlyList<string> sources)
		{
			var originalSourcePosition = OriginalColumnNumber.HasValue && OriginalLineNumber.HasValue
				? new SourcePosition(OriginalLineNumber.Value, OriginalColumnNumber.Value)
				: SourcePosition.NotFound;

			string? originalName = null;
			if (OriginalNameIndex.HasValue)
			{
				if (OriginalNameIndex.Value < 0 || OriginalNameIndex.Value >= names.Count)
				{
					throw new ArgumentOutOfRangeException($"Source map contains original name index (={OriginalNameIndex.Value}) that is outside the range of the provided names array[{names.Count}]");
				}

				originalName = names[OriginalNameIndex.Value];
			}

			string? originalFileName = null;
			if (OriginalSourceFileIndex.HasValue)
			{
				if (OriginalSourceFileIndex.Value < 0 || OriginalSourceFileIndex.Value >= sources.Count)
				{
					throw new ArgumentOutOfRangeException($"Source map contains original name index (={OriginalSourceFileIndex.Value}) that is outside the range of the provided names array[{sources.Count}]");
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
}
