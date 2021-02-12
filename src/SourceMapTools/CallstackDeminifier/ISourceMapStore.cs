using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	internal interface ISourceMapStore
	{
		SourceMap? GetSourceMapForUrl(string? sourceCodeUrl);
	}
}
