using System;
using System.Collections.Generic;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Corresponds to a single parsed entry in the source map mapping string that is used internally by the parser.
	/// The public API exposes the MappingEntry object, which is more useful to consumers of the library.
	/// </summary>
	internal struct NumericMappingEntry
	{
		/// <summary>
		/// The zero-based line number in the generated code that corresponds to this mapping segment.
		/// </summary>
		public int GeneratedLineNumber { get; internal set; }

		/// <summary>
		/// The zero-based column number in the generated code that corresponds to this mapping segment.
		/// </summary>
		public int GeneratedColumnNumber { get; internal set; }

		/// <summary>
		/// The zero-based index into the sources array that corresponds to this mapping segment.
		/// </summary>
		public int? OriginalSourceFileIndex { get; internal set; }

		/// <summary>
		/// The zero-based line number in the source code that corresponds to this mapping segment.
		/// </summary>
		public int? OriginalLineNumber { get; internal set; }

		/// <summary>
		/// The zero-based line number in the source code that corresponds to this mapping segment.
		/// </summary>
		public int? OriginalColumnNumber { get; internal set; }

		/// <summary>
		/// The zero-based index into the names array that can be used to identify names associated with this object.
		/// </summary>
		public int? OriginalNameIndex { get; internal set; }

		public MappingEntry ToMappingEntry(IReadOnlyList<string> names, IReadOnlyList<string> sources)
		{
			SourcePosition originalSourcePosition;

			if (OriginalColumnNumber.HasValue && OriginalLineNumber.HasValue)
			{
				originalSourcePosition = new SourcePosition(OriginalLineNumber.Value, OriginalColumnNumber.Value);
			}
			else
			{
				originalSourcePosition = SourcePosition.NotFound;
			}

			string? originalName = null;
			if (OriginalNameIndex.HasValue)
			{
				try
				{
					originalName = names[OriginalNameIndex.Value];
				}
				catch (IndexOutOfRangeException e)
				{
					throw new IndexOutOfRangeException("Source map contains original name index that is outside the range of the provided names array", e);
				}

			}

			string? originalFileName = null;
			if (OriginalSourceFileIndex.HasValue)
			{
				try
				{
					originalFileName = sources[OriginalSourceFileIndex.Value];
				}
				catch (IndexOutOfRangeException e)
				{
					throw new IndexOutOfRangeException("Source map contains original source index that is outside the range of the provided sources array", e);
				}
			}

			return new MappingEntry(
				new SourcePosition(GeneratedLineNumber, GeneratedColumnNumber),
				originalSourcePosition,
				originalName,
				originalFileName);
		}
	}
}
