using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class StackTraceParserUnitTests
	{
		[Test]
		public void ParseStackTrace_ChromeCallstack_GenerateCorrectNumberOfStackFrames()
		{
			// Arrange
			IStackTraceParser stackTraceParser = new StackTraceParser();
			var browserStackTrace = @"TypeError: Cannot read property 'length' of undefined
	at d (http://localhost:19220/crashcauser.min.js:1:75)
	at c (http://localhost:19220/crashcauser.min.js:1:34)
	at b (http://localhost:19220/crashcauser.min.js:1:14)
	at HTMLButtonElement.<anonymous> (http://localhost:19220/crashcauser.min.js:1:332)";

			// Act
			var stackTrace = stackTraceParser.ParseStackTrace(browserStackTrace);

			// Assert
			Assert.AreEqual(4, stackTrace.Count);
		}

		[Test]
		public void ParseStackTrace_FireFoxCallstack_GenerateCorrectNumberOfStackFrames()
		{
			// Arrange
			IStackTraceParser stackTraceParser = new StackTraceParser();
			var browserStackTrace = @"d @http://localhost:19220/crashcauser.min.js:1:68
c@http://localhost:19220/crashcauser.min.js:1:34
b@http://localhost:19220/crashcauser.min.js:1:14
window.onload/<@http://localhost:19220/crashcauser.min.js:1:332";

			// Act
			var stackTrace = stackTraceParser.ParseStackTrace(browserStackTrace);

			// Assert
			Assert.AreEqual(4, stackTrace.Count);
		}

		[Test]
		public void ParseStackTrace_InternetExplorer11Callstack_GenerateCorrectNumberOfStackFrames()
		{
			// Arrange
			IStackTraceParser stackTraceParser = new StackTraceParser();
			var browserStackTrace = @"TypeError: Unable to get property 'length' of undefined or null reference
   at d (http://localhost:19220/crashcauser.min.js:1:55)
   at c (http://localhost:19220/crashcauser.min.js:1:34)
   at b (http://localhost:19220/crashcauser.min.js:1:14)";

			// Act
			var stackTrace = stackTraceParser.ParseStackTrace(browserStackTrace);

			// Assert
			Assert.AreEqual(3, stackTrace.Count);
		}

		[Test]
		public void TryParseSingleStackFrame_EmptyString_ReturnNull()
		{
			// Arrange
			var frame = string.Empty;

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame);

			// Assert
			Assert.Null(result);
		}

		[Test]
		public void TryParseSingleStackFrame_NoMethodNameInInput_ReturnsStackFrameWithNullMethod()
		{
			// Arrange
			var frame = "    (http://localhost:19220/crashcauser.min.js:1:34)";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.Null(result.MethodName);
			Assert.AreEqual(0, result.SourcePosition!.Line);
			Assert.AreEqual(33, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_StackFrameWithWebpackLink_CorrectStackFrame()
		{
			// Arrange
			var frame = "    at eval (webpack-internal:///./Static/jsx/InitialStep/InitialStepForm.js:167:14)";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("webpack-internal:///./Static/jsx/InitialStep/InitialStepForm.js", result.FilePath);
			Assert.AreEqual("eval", result.MethodName);
			Assert.AreEqual(167 - 1, result.SourcePosition!.Line);
			Assert.AreEqual(14 - 1, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_StackFrameWithoutParentheses_CorrectStackFrame()
		{
			// Arrange
			var frame = "    at c http://localhost:19220/crashcauser.min.js:8:3";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.AreEqual("c", result.MethodName);
			Assert.AreEqual(7, result.SourcePosition!.Line);
			Assert.AreEqual(2, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_ChromeStackFrame_CorrectStackFrame()
		{
			// Arrange
			var frame = "    at c (http://localhost:19220/crashcauser.min.js:8:3)";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.AreEqual("c", result.MethodName);
			Assert.AreEqual(7, result.SourcePosition!.Line);
			Assert.AreEqual(2, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_ChromeStackFrameWithNoMethodName_CorrectStackFrame()
		{
			// Arrange
			var frame = " at http://localhost:19220/crashcauser.min.js:10:13";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.Null(result.MethodName);
			Assert.AreEqual(9, result.SourcePosition!.Line);
			Assert.AreEqual(12, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_ChromeStackFrameWithScriptSubfolder_CorrectStackFrame()
		{
			// Arrange
			var frame = "    at c (http://localhost:19220/o/app_scripts/crashcauser.min.js:9:5)";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/o/app_scripts/crashcauser.min.js", result.FilePath);
			Assert.AreEqual("c", result.MethodName);
			Assert.AreEqual(8, result.SourcePosition!.Line);
			Assert.AreEqual(4, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_FireFoxStackFrame_CorrectStackFrame()
		{
			// Arrange
			var frame = "c@http://localhost:19220/crashcauser.min.js:4:52";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.AreEqual("c", result.MethodName);
			Assert.AreEqual(3, result.SourcePosition!.Line);
			Assert.AreEqual(51, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_IE11StackFrame_CorrectStackFrame()
		{
			// Arrange
			var frame = "   at c (http://localhost:19220/crashcauser.min.js:3:17)";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.AreEqual("c", result.MethodName);
			Assert.AreEqual(2, result.SourcePosition!.Line);
			Assert.AreEqual(16, result.SourcePosition.Column);
		}

		[Test]
		public void TryParseSingleStackFrame_IE11StackFrameWithAnonymousFunction_CorrectStackFrame()
		{
			// Arrange
			var frame = "   at Anonymous function (http://localhost:19220/crashcauser.min.js:5:25)";

			// Act
			var result = StackTraceParser.TryParseSingleStackFrame(frame)!;

			// Assert
			Assert.AreEqual("http://localhost:19220/crashcauser.min.js", result.FilePath);
			Assert.AreEqual("Anonymous function", result.MethodName);
			Assert.AreEqual(4, result.SourcePosition!.Line);
			Assert.AreEqual(24, result.SourcePosition.Column);
		}
	}
}