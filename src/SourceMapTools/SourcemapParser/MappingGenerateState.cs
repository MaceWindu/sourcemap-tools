using System.Collections.Generic;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Class to track the internal state during source map serialize
	/// </summary>
	internal sealed class MappingGenerateState
	{
		/// <summary>
		/// Last location of the code in the transformed code
		/// </summary>
		public SourcePosition LastGeneratedPosition { get; private set; }

		/// <summary>
		/// Last location of the code in the source code
		/// </summary>
		public SourcePosition LastOriginalPosition { get; internal set; }

		/// <summary>
		/// List that contains the symbol names
		/// </summary>
		public IReadOnlyList<string> Names { get; }

		/// <summary>
		/// List that contains the file sources
		/// </summary>
		public IReadOnlyList<string> Sources { get; }

		/// <summary>
		/// Index of last file source
		/// </summary>
		public int LastSourceIndex { get; set; }

		/// <summary>
		/// Index of last symbol name
		/// </summary>
		public int LastNameIndex { get; set; }

		/// <summary>
		/// Whether this is the first segment in current line
		/// </summary>
		public bool IsFirstSegment { get; set; }

		public MappingGenerateState(IReadOnlyList<string> names, IReadOnlyList<string> sources)
		{
			Names = names;
			Sources = sources;
			IsFirstSegment = true;
		}

		public void AdvanceLastGeneratedPositionLine() => LastGeneratedPosition = new SourcePosition(LastGeneratedPosition.Line + 1, 0);

		public void UpdateLastGeneratedPositionColumn(int zeroBasedColumnNumber) => LastGeneratedPosition = new SourcePosition(LastGeneratedPosition.Line, zeroBasedColumnNumber);
	}
}
