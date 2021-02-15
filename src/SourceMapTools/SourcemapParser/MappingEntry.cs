namespace SourcemapToolkit.SourcemapParser
{
	public struct MappingEntry
	{
		public MappingEntry(SourcePosition generatedSourcePosition)
			: this(generatedSourcePosition, null, null, null)
		{
		}

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

		public MappingEntry CloneWithResetColumnNumber()
		{
			return new MappingEntry(
				new SourcePosition(GeneratedSourcePosition.Line, 0),
				new SourcePosition(OriginalSourcePosition.Line, 0),
				OriginalName,
				OriginalFileName);
		}

		public bool IsValueEqual(MappingEntry anEntry)
		{
			return
				OriginalName == anEntry.OriginalName &&
				OriginalFileName == anEntry.OriginalFileName &&
				GeneratedSourcePosition.Equals(anEntry.GeneratedSourcePosition) &&
				OriginalSourcePosition.Equals(anEntry.OriginalSourcePosition);
		}

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