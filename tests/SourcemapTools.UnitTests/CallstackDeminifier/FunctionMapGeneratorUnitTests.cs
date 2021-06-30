using System;
using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;
using SourcemapToolkit.SourcemapParser.UnitTests;
using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class FunctionMapGeneratorUnitTests
	{
		[Test]
		public void GenerateFunctionMap_NullSourceMap_ReturnsNull()
		{
			// Arrange
			IFunctionMapGenerator functionMapGenerator = new FunctionMapGenerator();
			var sourceCode = "";

			// Act
			var functionMap = functionMapGenerator.GenerateFunctionMap(UnitTestUtils.StreamFromString(sourceCode), null);

			// Assert
			Assert.Null(functionMap);
		}


		[Test]
		public void ParseSourceCode_NoFunctionsInSource_EmptyFunctionList()
		{
			// Arrange
			var sourceCode = "bar();";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(0, functionMap.Count);
		}

		[Test]
		public void ParseSourceCode_SingleLineFunctionInSource_CorrectZeroBasedColumnNumbers()
		{
			// Arrange
			var sourceCode = "function foo(){bar();}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(1, functionMap.Count);
			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(9, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(14, functionMap[0].Start.Column);
			Assert.AreEqual(22, functionMap[0].End.Column);
		}

		[Test]
		public void ParseSourceCode_MultiLineFunctionInSource_CorrectColumnAndZeroBasedLineNumbers()
		{
			// Arrange
			var sourceCode = "function foo()" + Environment.NewLine + "{" + Environment.NewLine + "bar();" +
								Environment.NewLine + "}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(1, functionMap.Count);
			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(9, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(1, functionMap[0].Start.Line);
			Assert.AreEqual(3, functionMap[0].End.Line);
			Assert.AreEqual(0, functionMap[0].Start.Column);
			Assert.AreEqual(1, functionMap[0].End.Column);
		}

		[Test]
		public void ParseSourceCode_TwoSingleLineFunctions_TwoFunctionMapEntries()
		{
			// Arrange
			var sourceCode = "function foo(){bar();}function bar(){baz();}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("bar", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(31, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(36, functionMap[0].Start.Column);
			Assert.AreEqual(44, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(9, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(14, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_TwoNestedSingleLineFunctions_TwoFunctionMapEntries()
		{
			// Arrange
			var sourceCode = "function foo(){function bar(){baz();}}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("bar", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(29, functionMap[0].Start.Column);
			Assert.AreEqual(37, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(9, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(14, functionMap[1].Start.Column);
			Assert.AreEqual(38, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_FunctionAssignedToVariable_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){bar();}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(1, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(20, functionMap[0].Start.Column);
			Assert.AreEqual(28, functionMap[0].End.Column);
		}

		[Test]
		public void ParseSourceCode_StaticMethod_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){};foo.bar = function() { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual("bar", functionMap[0].Bindings[1].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(23, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(44, functionMap[0].Start.Column);
			Assert.AreEqual(54, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(20, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_InstanceMethod_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype.bar = function () { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual("prototype", functionMap[0].Bindings[1].Name);
			Assert.AreEqual("bar", functionMap[0].Bindings[2].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(56, functionMap[0].Start.Column);
			Assert.AreEqual(66, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(20, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_InstanceMethodInObjectInitializer_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype = { bar: function () { baz(); } }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual("prototype", functionMap[0].Bindings[1].Name);
			Assert.AreEqual("bar", functionMap[0].Bindings[2].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[2].SourcePosition.Line);
			Assert.AreEqual(42, functionMap[0].Bindings[2].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(59, functionMap[0].Start.Column);
			Assert.AreEqual(69, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(20, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_FunctionAssignedToVariableAndHasName_FunctionMapEntryGeneratedForVariableName()
		{
			// Arrange
			var sourceCode = "var foo = function myCoolFunctionName(){ bar(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(1, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(39, functionMap[0].Start.Column);
			Assert.AreEqual(49, functionMap[0].End.Column);
		}

		[Test]
		public void ParseSourceCode_StaticMethodAndFunctionHasName_FunctionMapEntryGeneratedForPropertyName()
		{
			// Arrange
			var sourceCode = "var foo = function(){};foo.bar = function myCoolFunctionName() { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual("bar", functionMap[0].Bindings[1].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(23, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(63, functionMap[0].Start.Column);
			Assert.AreEqual(73, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(20, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_InstanceMethodAndFunctionHasName_FunctionMapEntryGeneratedForObjectPrototype()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype.bar = function myCoolFunctionName() { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual("prototype", functionMap[0].Bindings[1].Name);
			Assert.AreEqual("bar", functionMap[0].Bindings[2].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(74, functionMap[0].Start.Column);
			Assert.AreEqual(84, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(20, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		[Test]
		public void ParseSourceCode_InstanceMethodWithObjectInitializerAndFunctionHasName_FunctionMapEntryGeneratedForObjectPrototype()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype = { bar: function myCoolFunctionName() { baz(); } }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.AreEqual(2, functionMap.Count);

			Assert.AreEqual("foo", functionMap[0].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.AreEqual("prototype", functionMap[0].Bindings[1].Name);
			Assert.AreEqual("bar", functionMap[0].Bindings[2].Name);
			Assert.AreEqual(0, functionMap[0].Bindings[2].SourcePosition.Line);
			Assert.AreEqual(42, functionMap[0].Bindings[2].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[0].Start.Line);
			Assert.AreEqual(0, functionMap[0].End.Line);
			Assert.AreEqual(77, functionMap[0].Start.Column);
			Assert.AreEqual(87, functionMap[0].End.Column);

			Assert.AreEqual("foo", functionMap[1].Bindings[0].Name);
			Assert.AreEqual(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.AreEqual(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.AreEqual(0, functionMap[1].Start.Line);
			Assert.AreEqual(0, functionMap[1].End.Line);
			Assert.AreEqual(20, functionMap[1].Start.Column);
			Assert.AreEqual(22, functionMap[1].End.Column);
		}

		private static SourceMap CreateSourceMapMock()
		{
			return new SourceMap(
				0 /* version */,
				default /* file */,
				default /* mappings */,
				default /* sources */,
				default /* names */,
				new List<MappingEntry>() /* parsedMappings */,
				default /* sourcesContent */);
		}
	}
}