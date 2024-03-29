﻿using SourcemapToolkit.SourcemapParser.UnitTests;
using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class StackTraceDeminifierEndToEndTests
{
	private const string GeneratedCodeString = "function causeCrash(){function n(){var n=16;n+=2;t(n)}function t(n){n=n+2;i(n)}function i(n){(function(){var t;console.log(t.length+n)})()}window.onerror=function(n,t,i,r,u){u?document.getElementById(\"callstackdisplay\").innerText=u.stack:window.event.error&&(document.getElementById(\"callstackdisplay\").innerText=window.event.error.stack)};n()}window.onload=function(){document.getElementById(\"crashbutton\").addEventListener(\"click\",function(){causeCrash()})};";
	private const string SourceMapString = /*lang=json,strict*/ "{\r\n\"version\":3,\r\n\"file\":\"crashcauser.min.js\",\r\n\"lineCount\":1,\r\n\"mappings\":\"AAAAA,SAASA,UAAU,CAAA,CACnB,CACIC,SAASA,CAAM,CAAA,CAAG,CACd,IAAIC,EAAwB,EAAE,CAC9BA,CAAsB,EAAG,CAAC,CAC1BC,CAAM,CAACD,CAAD,CAHQ,CAMlBC,SAASA,CAAM,CAACC,CAAD,CAAQ,CACnBA,CAAM,CAAEA,CAAM,CAAE,CAAC,CACjBC,CAAM,CAACD,CAAD,CAFa,CAKvBC,SAASA,CAAM,CAACD,CAAD,CAAQ,EAClB,QAAQ,CAAA,CAAG,CACR,IAAIE,CAAC,CACLC,OAAOC,IAAI,CAACF,CAACG,OAAQ,CAAEL,CAAZ,CAFH,EAGX,CAAA,CAJkB,CAOvBM,MAAMC,QAAS,CAAEC,QAAS,CAACC,CAAO,CAAEC,CAAM,CAAEC,CAAM,CAAEC,CAAK,CAAEC,CAAjC,CAAwC,CAC1DA,CAAJ,CACIC,QAAQC,eAAe,CAAC,kBAAD,CAAoBC,UAAW,CAAEH,CAAKI,MADjE,CAESX,MAAMY,MAAML,M,GACjBC,QAAQC,eAAe,CAAC,kBAAD,CAAoBC,UAAW,CAAEV,MAAMY,MAAML,MAAMI,OAJhB,C,CAOlEpB,CAAM,CAAA,CA1BV,CA6BAS,MAAMa,OAAQ,CAAEC,QAAS,CAAA,CAAQ,CAC7BN,QAAQC,eAAe,CAAC,aAAD,CAAeM,iBAAiB,CAAC,OAAO,CAAE,QAAS,CAAA,CAAG,CACzEzB,UAAU,CAAA,CAD+D,CAAtB,CAD1B,C\",\r\n\"sources\":[\"crashcauser.js\"],\r\n\"names\":[\"causeCrash\",\"level1\",\"longLocalVariableName\",\"level2\",\"input\",\"level3\",\"x\",\"console\",\"log\",\"length\",\"window\",\"onerror\",\"window.onerror\",\"message\",\"source\",\"lineno\",\"colno\",\"error\",\"document\",\"getElementById\",\"innerText\",\"stack\",\"event\",\"onload\",\"window.onload\",\"addEventListener\"]\r\n}";

	private static StackTraceDeminifier GetStackTraceDeminifierWithDependencies()
	{
		const string url = "http://localhost:11323/crashcauser.min.js";
		var sourceMapProvider = new ISourceMapProviderMock(x => x == url ? UnitTestUtils.StreamFromString(SourceMapString) : null);
		var sourceCodeProvider = new ISourceCodeProviderMock(x => x == url ? UnitTestUtils.StreamFromString(GeneratedCodeString) : null);

		return StackTraceDeminifierFactory.GetStackTraceDeminifier(sourceMapProvider, sourceCodeProvider);
	}

	private static void ValidateDeminifyStackTraceResults(DeminifyStackTraceResult results, bool preferSourceMapsSymbols, string? topSymbolOverride = null)
	{
		Assert.That(results.DeminifiedStackFrameResults, Has.Count.EqualTo(6));
		Assert.Multiple(() =>
		{
			Assert.That(results.DeminifiedStackFrameResults[0].DeminificationError, Is.EqualTo(DeminificationError.None));
			Assert.That(results.DeminifiedStackFrameResults[0].DeminifiedStackFrame.MethodName, Is.EqualTo(topSymbolOverride ?? (preferSourceMapsSymbols ? "=> console" : "level3")));
			Assert.That(results.DeminifiedStackFrameResults[1].DeminifiedStackFrame.MethodName, Is.EqualTo("level3"));
			Assert.That(results.DeminifiedStackFrameResults[2].DeminifiedStackFrame.MethodName, Is.EqualTo("level2"));
			Assert.That(results.DeminifiedStackFrameResults[3].DeminifiedStackFrame.MethodName, Is.EqualTo("level1"));
			Assert.That(results.DeminifiedStackFrameResults[4].DeminifiedStackFrame.MethodName, Is.EqualTo("causeCrash"));
			Assert.That(results.DeminifiedStackFrameResults[5].DeminifiedStackFrame.MethodName, Is.EqualTo(preferSourceMapsSymbols ? null : "window.onload"));
		});
	}

	[Test]
	public void DeminifyStackTrace_ChromeStackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var chromeStackTrace = @"TypeError: Cannot read property 'length' of undefined
	at http://localhost:11323/crashcauser.min.js:1:125
	at i (http://localhost:11323/crashcauser.min.js:1:137)
	at t (http://localhost:11323/crashcauser.min.js:1:75)
	at n (http://localhost:11323/crashcauser.min.js:1:50)
	at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
	at HTMLButtonElement.<anonymous> (http://localhost:11323/crashcauser.min.js:1:445)";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(chromeStackTrace, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols, preferSourceMapsSymbols ? "=> length" : null);
	}

	[Test]
	public void DeminifyStackTrace_FireFoxStackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var fireFoxStackTrace = @"i/<@http://localhost:11323/crashcauser.min.js:1:112
i@http://localhost:11323/crashcauser.min.js:1:95
t@http://localhost:11323/crashcauser.min.js:1:75
n@http://localhost:11323/crashcauser.min.js:1:50
causeCrash@http://localhost:11323/crashcauser.min.js:1:341
window.onload/<@http://localhost:11323/crashcauser.min.js:1:445";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(fireFoxStackTrace, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols);
	}

	[Test]
	public void DeminifyStackTrace_IE11StackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var ieStackTrace = @"TypeError: Unable to get property 'length' of undefined or null reference
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:112)
   at i (http://localhost:11323/crashcauser.min.js:1:95)
   at t (http://localhost:11323/crashcauser.min.js:1:75)
   at n (http://localhost:11323/crashcauser.min.js:1:50)
   at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:445)";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(ieStackTrace, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols);
	}

	[Test]
	public void DeminifyStackTrace_EdgeStackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var dgeStackTrace = @"TypeError: Unable to get property 'length' of undefined or null reference
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:112)
   at i (http://localhost:11323/crashcauser.min.js:1:95)
   at t (http://localhost:11323/crashcauser.min.js:1:75)
   at n (http://localhost:11323/crashcauser.min.js:1:50)
   at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:445)";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(dgeStackTrace, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols);
	}

	[Test]
	public void DeminifyResultToString_SuccessfullyDeminified_AllLinesDeminified([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var ieStackTrace = @"TypeError: Unable to get property 'length' of undefined or null reference
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:112)
   at i (http://localhost:11323/crashcauser.min.js:1:95)
   at t (http://localhost:11323/crashcauser.min.js:1:75)
   at n (http://localhost:11323/crashcauser.min.js:1:50)
   at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:445)";
		var results = stackTraceDeminifier.DeminifyStackTrace(ieStackTrace, preferSourceMapsSymbols);
		var expectedResult = !preferSourceMapsSymbols
			? @"TypeError: Unable to get property 'length' of undefined or null reference
  at level3 in crashcauser.js:17:13
  at level3 in crashcauser.js:15:10
  at level2 in crashcauser.js:11:9
  at level1 in crashcauser.js:6:9
  at causeCrash in crashcauser.js:28:5
  at window.onload in crashcauser.js:33:9"
			: @"TypeError: Unable to get property 'length' of undefined or null reference
  at => console in crashcauser.js:17:13
  at level3 in crashcauser.js:15:10
  at level2 in crashcauser.js:11:9
  at level1 in crashcauser.js:6:9
  at causeCrash in crashcauser.js:28:5
  at Anonymous function in crashcauser.js:33:9";

		// Act
		var formatted = results.ToString();

		// Assert
		Assert.That(formatted.Replace("\r", ""), Is.EqualTo(expectedResult.Replace("\r", "")));
	}
}