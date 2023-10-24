using System;
using System.Collections.Generic;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class IFunctionMapStoreMock(Func<string?, IReadOnlyList<FunctionMapEntry>?> getFunctionMapForSourceCode) : IFunctionMapStore
{
	public IReadOnlyList<FunctionMapEntry>? GetFunctionMapForSourceCode(string? sourceCodeUrl) => getFunctionMapForSourceCode(sourceCodeUrl);
}
