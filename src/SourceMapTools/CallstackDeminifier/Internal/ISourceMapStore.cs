using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Internal API.
/// </summary>
public interface ISourceMapStore
{
	/// <summary>
	/// Internal API.
	/// </summary>
	public SourceMap? GetSourceMapForUrl(string? sourceCodeUrl);
}
