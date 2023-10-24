using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// Class to track the internal state during source map serialize.
/// </summary>
public sealed class MappingGenerateState(IReadOnlyList<string> names, IReadOnlyList<string> sources)
{
	/// <summary>
	/// Last location of the code in the transformed code.
	/// </summary>
	internal SourcePosition LastGeneratedPosition { get; private set; }

	/// <summary>
	/// Last location of the code in the source code.
	/// </summary>
	internal SourcePosition LastOriginalPosition { get; set; }

	/// <summary>
	/// List that contains the symbol names.
	/// </summary>
	public IReadOnlyList<string> Names { get; } = names;

	/// <summary>
	/// List that contains the file sources.
	/// </summary>
	public IReadOnlyList<string> Sources { get; } = sources;

	/// <summary>
	/// Index of last file source.
	/// </summary>
	internal int LastSourceIndex { get; set; }

	/// <summary>
	/// Index of last symbol name.
	/// </summary>
	internal int LastNameIndex { get; set; }

	/// <summary>
	/// Whether this is the first segment in current line.
	/// </summary>
	public bool IsFirstSegment { get; set; } = true;

	/// <summary>
	/// Internal API.
	/// </summary>
	public void AdvanceLastGeneratedPositionLine() => LastGeneratedPosition = new SourcePosition(LastGeneratedPosition.Line + 1, 0);

	/// <summary>
	/// Internal API.
	/// </summary>
	public void UpdateLastGeneratedPositionColumn(int zeroBasedColumnNumber) => LastGeneratedPosition = new SourcePosition(LastGeneratedPosition.Line, zeroBasedColumnNumber);
}
