using System;
using System.IO;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class ISourceMapProviderMock(Func<string, Stream?> getSourceMapContentsForCallstackUrl) : ISourceMapProvider
{
	Stream? ISourceMapProvider.GetSourceMapContentsForCallstackUrl(string correspondingCallStackFileUrl) => getSourceMapContentsForCallstackUrl(correspondingCallStackFileUrl);
}
