using System;
using System.IO;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class ISourceCodeProviderMock(Func<string, Stream?> getSourceCode) : ISourceCodeProvider
{
	Stream? ISourceCodeProvider.GetSourceCode(string sourceCodeUrl) => getSourceCode(sourceCodeUrl);
}
