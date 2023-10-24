using System;
using System.Collections.Generic;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

internal sealed class IStackTraceParserMock(Func<string, IReadOnlyList<StackFrame>> parseStackTrace) : IStackTraceParser
{
	IReadOnlyList<StackFrame> IStackTraceParser.ParseStackTrace(string stackTraceString) => parseStackTrace(stackTraceString);
	IReadOnlyList<StackFrame> IStackTraceParser.ParseStackTrace(string stackTraceString, out string? message)
	{
		message = null;
		return parseStackTrace(stackTraceString);
	}
}
