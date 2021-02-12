using System.Collections.Generic;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Class to track the internal state during source map serialize
	/// </summary>
	internal class MappingGenerateState
	{
		/// <summary>
		/// Last location of the code in the transformed code
		/// </summary>
		public SourcePosition LastGeneratedPosition { get; private set; } = new SourcePosition();

		/// <summary>
		/// Last location of the code in the source code
		/// </summary>
		public SourcePosition LastOriginalPosition { get; } = new SourcePosition();

		/// <summary>
		/// List that contains the symbol names
		/// </summary>
		public IList<string> Names { get; }

		/// <summary>
		/// List that contains the file sources
		/// </summary>
		public IList<string> Sources { get; }

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

		public MappingGenerateState(IList<string> names, IList<string> sources)
		{
			Names = names;
			Sources = sources;
			IsFirstSegment = true;
		}
	}
}
