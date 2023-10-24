using System;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class IStackFrameDeminifierMock(Func<StackFrame, string?, bool, StackFrameDeminificationResult> deminifyStackFrame)
	: IStackFrameDeminifier
{
	StackFrameDeminificationResult IStackFrameDeminifier.DeminifyStackFrame(StackFrame stackFrame, string? callerSymbolName, bool preferSourceMapsSymbols)
		=> deminifyStackFrame(stackFrame, callerSymbolName, preferSourceMapsSymbols);
}
