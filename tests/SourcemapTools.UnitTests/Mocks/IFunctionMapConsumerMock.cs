using System;
using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class IFunctionMapConsumerMock(Func<SourcePosition, IReadOnlyList<FunctionMapEntry>, FunctionMapEntry?> getWrappingFunctionForSourceLocation)
	: IFunctionMapConsumer
{
	FunctionMapEntry? IFunctionMapConsumer.GetWrappingFunctionForSourceLocation(
		SourcePosition sourcePosition,
		IReadOnlyList<FunctionMapEntry> functionMap) => getWrappingFunctionForSourceLocation(sourcePosition, functionMap);
}
