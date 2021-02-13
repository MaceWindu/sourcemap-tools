namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// The various fields within a segment of the Mapping parser are relative to the previous value we parsed.
	/// This class tracks this state throughout the parsing process. 
	/// </summary>
	internal struct MappingsParserState
	{
		public readonly int CurrentGeneratedLineNumber;
		public readonly int CurrentGeneratedColumnBase;
		public readonly int SourcesListIndexBase;
		public readonly int OriginalSourceStartingLineBase;
		public readonly int OriginalSourceStartingColumnBase;
		public readonly int NamesListIndexBase;

		public MappingsParserState(MappingsParserState previousMappingsParserState = new MappingsParserState(),
			int? newGeneratedLineNumber = null,
			int? newGeneratedColumnBase = null,
			int? newSourcesListIndexBase = null,
			int? newOriginalSourceStartingLineBase = null,
			int? newOriginalSourceStartingColumnBase = null,
			int? newNamesListIndexBase = null)
		{
			CurrentGeneratedLineNumber = newGeneratedLineNumber ?? previousMappingsParserState.CurrentGeneratedLineNumber;
			CurrentGeneratedColumnBase = newGeneratedColumnBase ?? previousMappingsParserState.CurrentGeneratedColumnBase;
			SourcesListIndexBase = newSourcesListIndexBase ?? previousMappingsParserState.SourcesListIndexBase;
			OriginalSourceStartingLineBase = newOriginalSourceStartingLineBase ?? previousMappingsParserState.OriginalSourceStartingLineBase;
			OriginalSourceStartingColumnBase = newOriginalSourceStartingColumnBase ?? previousMappingsParserState.OriginalSourceStartingColumnBase;
			NamesListIndexBase = newNamesListIndexBase ?? previousMappingsParserState.NamesListIndexBase;
		}
	}
}
