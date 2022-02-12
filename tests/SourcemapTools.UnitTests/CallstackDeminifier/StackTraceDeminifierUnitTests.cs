using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class StackTraceDeminifierUnitTests
	{
		[Test]
		public void DeminifyStackTrace_UnableToParseStackTraceString_ReturnsEmptyList([Values] bool preferSourceMapsSymbols)
		{
			// Arrange
			var stackTraceParser = new Mock<IStackTraceParser>();
			var stackTraceString = "foobar";
			var message = "Error example";
			stackTraceParser.Setup(x => x.ParseStackTrace(stackTraceString, out message)).Returns(new List<StackFrame>());

			var stackFrameDeminifier = new Mock<IStackFrameDeminifier>().Object;

			var stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser.Object);

			// Act
			var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString, preferSourceMapsSymbols);

			// Assert
			Assert.AreEqual(0, result.DeminifiedStackFrameResults.Count);
		}

		[Test]
		public void DeminifyStackTrace_AbleToDeminifyStackTrace_ResultContainsDeminifiedFrame([Values] bool preferSourceMapsSymbols)
		{
			// Arrange
			var stackTraceParser = new Mock<IStackTraceParser>();
			var minifiedStackFrames = new List<StackFrame> { new StackFrame(null) };
			var stackTraceString = "foobar";
			var message = "Error example";
			stackTraceParser.Setup(x => x.ParseStackTrace(stackTraceString, out message)).Returns(minifiedStackFrames);

			var stackFrameDeminifier = new Mock<IStackFrameDeminifier>();
			var stackFrameDeminification = new StackFrameDeminificationResult(default, null!);
			stackFrameDeminifier.Setup(x => x.DeminifyStackFrame(minifiedStackFrames[0], null, preferSourceMapsSymbols)).Returns(stackFrameDeminification);

			var stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier.Object, stackTraceParser.Object);

			// Act
			var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString, preferSourceMapsSymbols);

			// Assert
			Assert.AreEqual(1, result.DeminifiedStackFrameResults.Count);
			Assert.AreEqual(minifiedStackFrames[0], result.MinifiedStackFrames[0]);
			Assert.AreEqual(stackFrameDeminification, result.DeminifiedStackFrameResults[0]);
		}
	}
}