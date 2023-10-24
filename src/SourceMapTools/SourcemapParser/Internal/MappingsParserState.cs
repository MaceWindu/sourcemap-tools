namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// The various fields within a segment of the Mapping parser are relative to the previous value we parsed.
/// This class tracks this state throughout the parsing process.
/// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct MappingsParserState(
#pragma warning restore CA1815 // Override equals and operator equals on value types
	MappingsParserState previousMappingsParserState = new MappingsParserState(),
	int? newGeneratedLineNumber = null,
	int? newGeneratedColumnBase = null,
	int? newSourcesListIndexBase = null,
	int? newOriginalSourceStartingLineBase = null,
	int? newOriginalSourceStartingColumnBase = null,
	int? newNamesListIndexBase = null)
{
	/// <summary>
	/// Internal API.
	/// </summary>
	internal int CurrentGeneratedLineNumber { get; } = newGeneratedLineNumber ?? previousMappingsParserState.CurrentGeneratedLineNumber;
	/// <summary>
	/// Internal API.
	/// </summary>
	internal int CurrentGeneratedColumnBase { get; } = newGeneratedColumnBase ?? previousMappingsParserState.CurrentGeneratedColumnBase;
	/// <summary>
	/// Internal API.
	/// </summary>
	internal int SourcesListIndexBase { get; } = newSourcesListIndexBase ?? previousMappingsParserState.SourcesListIndexBase;
	/// <summary>
	/// Internal API.
	/// </summary>
	internal int OriginalSourceStartingLineBase { get; } = newOriginalSourceStartingLineBase ?? previousMappingsParserState.OriginalSourceStartingLineBase;
	/// <summary>
	/// Internal API.
	/// </summary>
	internal int OriginalSourceStartingColumnBase { get; } = newOriginalSourceStartingColumnBase ?? previousMappingsParserState.OriginalSourceStartingColumnBase;
	/// <summary>
	/// Internal API.
	/// </summary>
	internal int NamesListIndexBase { get; } = newNamesListIndexBase ?? previousMappingsParserState.NamesListIndexBase;
}
