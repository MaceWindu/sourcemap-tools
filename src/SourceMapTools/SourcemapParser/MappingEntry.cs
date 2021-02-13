namespace SourcemapToolkit.SourcemapParser
{
	public class MappingEntry
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
			OriginalSourcePosition = originalSourcePosition;
			OriginalName = originalName;
			OriginalFileName = originalFileName;
		}

		/// <summary>
		/// The location of the line of code in the transformed code
		/// </summary>
		public SourcePosition GeneratedSourcePosition { get; internal set; }

		/// <summary>
		/// The location of the code in the original source code
		/// </summary>
		public SourcePosition? OriginalSourcePosition { get; internal set; }

		/// <summary>
		/// The original name of the code referenced by this mapping entry
		/// </summary>
		public string? OriginalName { get; internal set; }

		/// <summary>
		/// The name of the file that originally contained this code
		/// </summary>
		public string? OriginalFileName { get; internal set; }

		public MappingEntry Clone()
		{
			return new MappingEntry(
				GeneratedSourcePosition.Clone(),
				OriginalSourcePosition?.Clone(),
				OriginalName,
				OriginalFileName);
		}

		public bool IsValueEqual(MappingEntry anEntry)
		{
			return
				OriginalName == anEntry.OriginalName &&
				OriginalFileName == anEntry.OriginalFileName &&
				GeneratedSourcePosition.CompareTo(anEntry.GeneratedSourcePosition) == 0 &&
				((OriginalSourcePosition == null && anEntry.OriginalSourcePosition == null)
				|| (OriginalSourcePosition != null && anEntry.OriginalSourcePosition != null && OriginalSourcePosition.CompareTo(anEntry.OriginalSourcePosition) == 0));
		}
	}
}