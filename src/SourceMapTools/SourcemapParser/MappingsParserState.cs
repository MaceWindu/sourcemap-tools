using System.Diagnostics.CodeAnalysis;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// The various fields within a segment of the Mapping parser are relative to the previous value we parsed.
	/// This class tracks this state throughout the parsing process. 
	/// </summary>
	[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not used in comparisons")]
	internal readonly struct MappingsParserState
	{
		public int CurrentGeneratedLineNumber { get; }
		public int CurrentGeneratedColumnBase { get; }
		public int SourcesListIndexBase { get; }
		public int OriginalSourceStartingLineBase { get; }
		public int OriginalSourceStartingColumnBase { get; }
		public int NamesListIndexBase { get; }

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
