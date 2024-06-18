using System;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class SourceMapMock : SourceMap
{
	private readonly Func<SourcePosition, Func<SourcePosition, MappingEntry?>, MappingEntry?>? _getMappingEntryForGeneratedSourcePosition;
	public SourceMapMock(Func<SourcePosition, Func<SourcePosition, MappingEntry?>, MappingEntry?> getMappingEntryForGeneratedSourcePosition)
		: this()
	{
		_getMappingEntryForGeneratedSourcePosition = getMappingEntryForGeneratedSourcePosition;
	}

	public SourceMapMock()
		: base(
			0 /* version */,
			default /* file */,
			default /* mappings */,
			default /* sources */,
			default /* names */,
			[] /* parsedMappings */,
			default /* sourcesContent */)
	{
	}

	public override MappingEntry? GetMappingEntryForGeneratedSourcePosition(SourcePosition generatedSourcePosition)
		=> _getMappingEntryForGeneratedSourcePosition != null
			? _getMappingEntryForGeneratedSourcePosition(generatedSourcePosition, base.GetMappingEntryForGeneratedSourcePosition)
			: base.GetMappingEntryForGeneratedSourcePosition(generatedSourcePosition);
}
