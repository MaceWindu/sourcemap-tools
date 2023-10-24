using System;
using SourcemapToolkit.SourcemapParser;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class ISourceMapStoreMock(Func<string?, SourceMap?> getSourceMapForUrl) : ISourceMapStore
{
	SourceMap? ISourceMapStore.GetSourceMapForUrl(string? sourceCodeUrl) => getSourceMapForUrl(sourceCodeUrl);
}
