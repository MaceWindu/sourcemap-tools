using System;
using System.Collections.Generic;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Corresponds to a single parsed entry in the source map mapping string that is used internally by the parser.
	/// The public API exposes the MappingEntry object, which is more useful to consumers of the library.
	/// </summary>
	internal class NumericMappingEntry
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
			var result = new MappingEntry(new SourcePosition(GeneratedLineNumber, GeneratedColumnNumber));

			if (OriginalColumnNumber.HasValue && OriginalLineNumber.HasValue)
			{
				result.OriginalSourcePosition = new SourcePosition(OriginalLineNumber.Value, OriginalColumnNumber.Value);
			}

			if (OriginalNameIndex.HasValue)
			{
				try
				{
					result.OriginalName = names[OriginalNameIndex.Value];
				}
				catch (IndexOutOfRangeException e)
				{
					throw new IndexOutOfRangeException("Source map contains original name index that is outside the range of the provided names array", e);
				}

			}

			if (OriginalSourceFileIndex.HasValue)
			{
				try
				{
					result.OriginalFileName = sources[OriginalSourceFileIndex.Value];
				}
				catch (IndexOutOfRangeException e)
				{
					throw new IndexOutOfRangeException("Source map contains original source index that is outside the range of the provided sources array", e);
				}
			}

			return result;
		}
	}
}
