using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class StackTraceDeminifierUnitTests
{
	[Test]
	public void DeminifyStackTrace_UnableToParseStackTraceString_ReturnsEmptyList([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackTraceString = "foobar";
		var stackTraceParser = new IStackTraceParserMock(x => x == stackTraceString ? new List<StackFrame>() : throw new InvalidOperationException());

		var stackFrameDeminifier = new IStackFrameDeminifierMock((_, _, _) => throw new NotImplementedException());

		var stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser);

		// Act
		var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString, preferSourceMapsSymbols);

		// Assert
		Assert.That(result.DeminifiedStackFrameResults, Is.Empty);
	}

	[Test]
	public void DeminifyStackTrace_AbleToDeminifyStackTrace_ResultContainsDeminifiedFrame([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var minifiedStackFrames = new List<StackFrame> { new(null) };
		var stackTraceString = "foobar";
		var stackTraceParser = new IStackTraceParserMock(x => x == stackTraceString ? minifiedStackFrames : throw new InvalidOperationException());

		var stackFrameDeminification = new StackFrameDeminificationResult(default, new StackFrame(null));
		var stackFrameDeminifier = new IStackFrameDeminifierMock((x, y, z) =>
			x == minifiedStackFrames[0] && y == null && z == preferSourceMapsSymbols ? stackFrameDeminification : throw new InvalidOperationException());

		var stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser);

		// Act
		var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.DeminifiedStackFrameResults, Has.Count.EqualTo(1));
			Assert.That(result.MinifiedStackFrames[0], Is.EqualTo(minifiedStackFrames[0]));
			Assert.That(result.DeminifiedStackFrameResults[0], Is.EqualTo(stackFrameDeminification));
		});
	}
}