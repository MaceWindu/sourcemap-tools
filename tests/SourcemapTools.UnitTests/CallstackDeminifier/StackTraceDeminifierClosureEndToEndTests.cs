﻿using NUnit.Framework;
using SourcemapToolkit.SourcemapParser.UnitTests;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class StackTraceDeminifierClosureEndToEndTests
{
	private const string GeneratedCodeString = "function a(){}window.foo=a;a.prototype={b:function(){return a.a(void 0)}};a.a=function(b){return b.length};function c(){return(new a).b()}window.foo.bar=a.b;window.foo.bar2=a.a;window.bar=c;window.onerror=function(b,e,f,g,d){d?document.getElementById(\"callstackdisplay\").innerText=d.stack:window.event.error&&(document.getElementById(\"callstackdisplay\").innerText=window.event.error.stack)};window.onload=function(){document.getElementById(\"crashbutton\").addEventListener(\"click\",function(){console.log(c())})};";
	private const string SourceMapString = /*lang=json,strict*/ "{\r\n\"version\":3,\r\n\"file\":\"\",\r\n\"lineCount\":1,\r\n\"mappings\":\"AAEgCA,QAAA,EAAS,EAAG,EAC5CC,MAAA,IAAA,CAAgBD,CAChBA,EAAAE,UAAA,CAA0C,CAAEC,EAAuBA,QAAS,EAAG,CAAS,MAAOC,EAAAC,EAAA,CAAVC,IAAAA,EAAU,CAAhB,CAArC,CAE1CF,EAAAC,EAAA,CAAqDD,QAAS,CAACG,CAAD,CAAI,CAAE,MAAOA,EAAAC,OAAT,CAElEC,SAASA,EAAc,EAAG,CAA+C,MAAON,CAA5CG,IAAIN,CAAwCG,GAAA,EAAtD,CAE1BF,MAAA,IAAA,IAAA,CAAuBS,CAAAP,EACvBF,OAAA,IAAA,KAAA,CAAwBG,CAAAC,EACxBJ,OAAA,IAAA,CAAgBQ,CAEhBR,OAAAU,QAAA,CAAiBC,QAAS,CAACC,CAAD,CAAUC,CAAV,CAAkBC,CAAlB,CAA0BC,CAA1B,CAAiCC,CAAjC,CAAwC,CAC1DA,CAAJ,CACIC,QAAAC,eAAA,CAAwB,kBAAxB,CAAAC,UADJ,CAC4DH,CAAAI,MAD5D,CAESpB,MAAAqB,MAAAL,MAFT,GAGIC,QAAAC,eAAA,CAAwB,kBAAxB,CAAAC,UAHJ,CAG4DnB,MAAAqB,MAAAL,MAAAI,MAH5D,CAD8D,CAOlEpB,OAAAsB,OAAA,CAAgBC,QAAS,EAAQ,CAC7BN,QAAAC,eAAA,CAAwB,aAAxB,CAAAM,iBAAA,CAAwD,OAAxD,CAAiE,QAAS,EAAG,CACzEC,OAAAC,IAAA,CAAYlB,CAAA,EAAZ,CADyE,CAA7E,CAD6B;\",\r\n\"sources\":[\"//officefile/public/thomabr/closurecrashcauser.js\"],\r\n\"names\":[\"mynamespace.objectWithMethods\",\"window\",\"prototype\",\"prototypeMethodLevel1\",\"mynamespace.objectWithMethods.propertyMethodLevel2\",\"propertyMethodLevel2\",\"x\",\"e\",\"length\",\"GlobalFunction\",\"mynamespace.objectWithMethods.prototypeMethodLevel1\",\"onerror\",\"window.onerror\",\"message\",\"source\",\"lineno\",\"colno\",\"error\",\"document\",\"getElementById\",\"innerText\",\"stack\",\"event\",\"onload\",\"window.onload\",\"addEventListener\",\"console\",\"log\"]\r\n}\r\n";

	private static StackTraceDeminifier GetStackTraceDeminifierWithDependencies()
	{
		const string url = "http://localhost:11323/closurecrashcauser.minified.js";
		var sourceMapProvider = new ISourceMapProviderMock(x => x == url ? UnitTestUtils.StreamFromString(SourceMapString) : null);
		var sourceCodeProvider = new ISourceCodeProviderMock(x => x == url ? UnitTestUtils.StreamFromString(GeneratedCodeString) : null);

		return StackTraceDeminifierFactory.GetStackTraceDeminifier(sourceMapProvider, sourceCodeProvider);
	}

	private static void ValidateDeminifyStackTraceResults(DeminifyStackTraceResult results, bool preferSourceMapsSymbols)
	{
		Assert.That(results.DeminifiedStackFrameResults, Has.Count.EqualTo(4));
		Assert.Multiple(() =>
		{
			Assert.That(results.DeminifiedStackFrameResults[0].DeminificationError, Is.EqualTo(DeminificationError.None));
			Assert.That(results.DeminifiedStackFrameResults[0].DeminifiedStackFrame.MethodName, Is.EqualTo(preferSourceMapsSymbols ? "propertyMethodLevel2 => length" : "mynamespace.objectWithMethods.propertyMethodLevel2"));
			Assert.That(results.DeminifiedStackFrameResults[1].DeminifiedStackFrame.MethodName, Is.EqualTo(preferSourceMapsSymbols ? "prototypeMethodLevel1" : "mynamespace.objectWithMethods.prototypeMethodLevel1"));
			Assert.That(results.DeminifiedStackFrameResults[2].DeminifiedStackFrame.MethodName, Is.EqualTo("GlobalFunction"));
			Assert.That(results.DeminifiedStackFrameResults[3].DeminifiedStackFrame.MethodName, Is.EqualTo(preferSourceMapsSymbols ? null : "window.onload"));
		});
	}

	[Test]
	public void DeminifyClosureStackTrace_ChromeStackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		if (preferSourceMapsSymbols)
		{
			Assert.Inconclusive($"preferSourceMapsSymbols doesn't work well with test inputs");
		}

		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var callstack = @"TypeError: Cannot read property 'length' of undefined
	at Function.a.a (http://localhost:11323/closurecrashcauser.minified.js:1:99)
	at a.b (http://localhost:11323/closurecrashcauser.minified.js:1:63)
	at c (http://localhost:11323/closurecrashcauser.minified.js:1:135)
	at HTMLButtonElement.<anonymous> (http://localhost:11323/closurecrashcauser.minified.js:1:504)";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(callstack, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols);
	}

	[Test]
	public void DeminifyClosureStackTrace_FireFoxStackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		if (preferSourceMapsSymbols)
		{
			Assert.Inconclusive($"preferSourceMapsSymbols doesn't work well with test inputs");
		}

		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var callstack = @"a.a@http://localhost:11323/closurecrashcauser.minified.js:1:91
a.prototype.b@http://localhost:11323/closurecrashcauser.minified.js:1:61
c@http://localhost:11323/closurecrashcauser.minified.js:1:128
window.onload/<@http://localhost:11323/closurecrashcauser.minified.js:1:504";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(callstack, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols);
	}

	[Test]
	public void DeminifyClosureStackTrace_IE11AndEdgeStackTraceString_CorrectDeminificationWhenPossible([Values] bool preferSourceMapsSymbols)
	{
		if (preferSourceMapsSymbols)
		{
			Assert.Inconclusive($"preferSourceMapsSymbols doesn't work well with test inputs");
		}

		// Arrange
		var stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
		var callstack = @"TypeError: Unable to get property 'length' of undefined or null reference
   at a.a (http://localhost:11323/closurecrashcauser.minified.js:1:91)
   at a.prototype.b (http://localhost:11323/closurecrashcauser.minified.js:1:54)
   at c (http://localhost:11323/closurecrashcauser.minified.js:1:121)
   at Anonymous function (http://localhost:11323/closurecrashcauser.minified.js:1:492)";

		// Act
		var results = stackTraceDeminifier.DeminifyStackTrace(callstack, preferSourceMapsSymbols);

		// Assert
		ValidateDeminifyStackTraceResults(results, preferSourceMapsSymbols);
	}
}
