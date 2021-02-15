namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Source map entry.
	/// </summary>
	public struct MappingEntry
	{
		/// <summary>
		/// Creates source map entry instance.
		/// </summary>
		/// <param name="generatedSourcePosition">Entry position in minified script.</param>
		public MappingEntry(SourcePosition generatedSourcePosition)
			: this(generatedSourcePosition, null, null, null)
		{
		}

		/// <summary>
		/// Creates source map entry instance.
		/// </summary>
		/// <param name="generatedSourcePosition">Entry position in minified script.</param>
		/// <param name="originalSourcePosition">Entry position in original script.</param>
		/// <param name="originalName">Original name of source map entry.</param>
		/// <param name="originalFileName">File name of original source file with entry code.</param>
		public MappingEntry(
			SourcePosition generatedSourcePosition,
			SourcePosition? originalSourcePosition,
			string? originalName,
			string? originalFileName)
		{
			GeneratedSourcePosition = generatedSourcePosition;
			OriginalSourcePosition = originalSourcePosition ?? SourcePosition.NotFound;
			OriginalName = originalName;
			OriginalFileName = originalFileName;
		}

		/// <summary>
		/// The location of the line of code in the transformed code
		/// </summary>
		public readonly SourcePosition GeneratedSourcePosition { get; }

		/// <summary>
		/// The location of the code in the original source code
		/// </summary>
		public readonly SourcePosition OriginalSourcePosition { get; }

		/// <summary>
		/// The original name of the code referenced by this mapping entry
		/// </summary>
		public readonly string? OriginalName { get; }

		/// <summary>
		/// The name of the file that originally contained this code
		/// </summary>
		public readonly string? OriginalFileName { get; }

		/// <summary>
		/// Returns copy of entry with source positions having zero as column number.
		/// </summary>
		/// <returns>Returns copy of current entry.</returns>
		public MappingEntry CloneWithResetColumnNumber()
		{
			return new MappingEntry(
				new SourcePosition(GeneratedSourcePosition.Line, 0),
				new SourcePosition(OriginalSourcePosition.Line, 0),
				OriginalName,
				OriginalFileName);
		}

		/// <summary>
		/// Compares current mapping entry whith another one.
		/// </summary>
		/// <param name="anEntry">Mapping entry to compare with.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: both entries are the same.</item>
		/// <item><c>false</c>: entries differ from each other.</item>
		/// </list>
		/// </returns>
		public bool IsValueEqual(MappingEntry anEntry)
		{
			return
				OriginalName == anEntry.OriginalName &&
				OriginalFileName == anEntry.OriginalFileName &&
				GeneratedSourcePosition.Equals(anEntry.GeneratedSourcePosition) &&
				OriginalSourcePosition.Equals(anEntry.OriginalSourcePosition);
		}

		/// <summary>
		/// Compares current mapping entry whith another one.
		/// </summary>
		/// <param name="obj">Mapping entry to compare with.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: both entries are the same.</item>
		/// <item><c>false</c>: entries differ from each other oth <paramref name="obj"/> is not an instance of <see cref="MappingEntry"/>.</item>
		/// </list>
		/// </returns>
		public override bool Equals(object? obj)
		{
			return obj is MappingEntry mappingEntry && IsValueEqual(mappingEntry);
		}

		/// <summary>
		/// An implementation of Josh Bloch's hashing algorithm from Effective Java.
		/// It is fast, offers a good distribution (with primes 23 and 31), and allocation free.
		/// </summary>
		public override int GetHashCode()
		{
			unchecked // Wrap to protect overflow
			{
				var hash = 23;
				hash = hash * 31 + GeneratedSourcePosition.GetHashCode();
				hash = hash * 31 + OriginalSourcePosition.GetHashCode();
				hash = hash * 31 + (OriginalName ?? "").GetHashCode();
				hash = hash * 31 + (OriginalFileName ?? "").GetHashCode();
				return hash;
			}
		}
	}
}