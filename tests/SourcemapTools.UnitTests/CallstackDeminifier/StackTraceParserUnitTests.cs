using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

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
		Assert.That(stackTrace, Has.Count.EqualTo(4));
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
		Assert.That(stackTrace, Has.Count.EqualTo(4));
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
		Assert.That(stackTrace, Has.Count.EqualTo(3));
	}

	[Test]
	public void TryParseSingleStackFrame_EmptyString_ReturnNull()
	{
		// Arrange
		var frame = string.Empty;

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void TryParseSingleStackFrame_NoMethodNameInInput_ReturnsStackFrameWithNullMethod()
	{
		// Arrange
		var frame = "    (http://localhost:19220/crashcauser.min.js:1:34)";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.Null);
		});
		Assert.Multiple(() =>
		{
			Assert.That(result.SourcePosition.Line, Is.EqualTo(0));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(33));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_StackFrameWithWebpackLink_CorrectStackFrame()
	{
		// Arrange
		var frame = "    at eval (webpack-internal:///./Static/jsx/InitialStep/InitialStepForm.js:167:14)";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("webpack-internal:///./Static/jsx/InitialStep/InitialStepForm.js"));
			Assert.That(result.MethodName, Is.EqualTo("eval"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(167 - 1));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(14 - 1));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_StackFrameWithoutParentheses_CorrectStackFrame()
	{
		// Arrange
		var frame = "    at c http://localhost:19220/crashcauser.min.js:8:3";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.EqualTo("c"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(7));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(2));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_ChromeStackFrame_CorrectStackFrame()
	{
		// Arrange
		var frame = "    at c (http://localhost:19220/crashcauser.min.js:8:3)";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.EqualTo("c"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(7));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(2));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_ChromeStackFrameWithNoMethodName_CorrectStackFrame()
	{
		// Arrange
		var frame = " at http://localhost:19220/crashcauser.min.js:10:13";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.Null);
		});
		Assert.Multiple(() =>
		{
			Assert.That(result.SourcePosition.Line, Is.EqualTo(9));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(12));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_ChromeStackFrameWithScriptSubfolder_CorrectStackFrame()
	{
		// Arrange
		var frame = "    at c (http://localhost:19220/o/app_scripts/crashcauser.min.js:9:5)";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/o/app_scripts/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.EqualTo("c"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(8));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(4));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_FireFoxStackFrame_CorrectStackFrame()
	{
		// Arrange
		var frame = "c@http://localhost:19220/crashcauser.min.js:4:52";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.EqualTo("c"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(3));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(51));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_IE11StackFrame_CorrectStackFrame()
	{
		// Arrange
		var frame = "   at c (http://localhost:19220/crashcauser.min.js:3:17)";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.EqualTo("c"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(2));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(16));
		});
	}

	[Test]
	public void TryParseSingleStackFrame_IE11StackFrameWithAnonymousFunction_CorrectStackFrame()
	{
		// Arrange
		var frame = "   at Anonymous function (http://localhost:19220/crashcauser.min.js:5:25)";

		// Act
		var result = StackTraceParser.TryParseSingleStackFrame(frame);

		Assert.That(result, Is.Not.Null);
		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.FilePath, Is.EqualTo("http://localhost:19220/crashcauser.min.js"));
			Assert.That(result.MethodName, Is.EqualTo("Anonymous function"));
			Assert.That(result.SourcePosition.Line, Is.EqualTo(4));
			Assert.That(result.SourcePosition.Column, Is.EqualTo(24));
		});
	}
}