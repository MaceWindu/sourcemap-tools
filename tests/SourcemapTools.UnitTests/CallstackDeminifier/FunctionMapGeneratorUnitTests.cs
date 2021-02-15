using System;
using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;
using SourcemapToolkit.SourcemapParser.UnitTests;
using Xunit;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class FunctionMapGeneratorUnitTests
	{
		[Fact]
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


		[Fact]
		public void ParseSourceCode_NoFunctionsInSource_EmptyFunctionList()
		{
			// Arrange
			var sourceCode = "bar();";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Empty(functionMap);
		}

		[Fact]
		public void ParseSourceCode_SingleLineFunctionInSource_CorrectZeroBasedColumnNumbers()
		{
			// Arrange
			var sourceCode = "function foo(){bar();}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Single(functionMap);
			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(9, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(14, functionMap[0].Start.Column);
			Assert.Equal(22, functionMap[0].End.Column);
		}

		[Fact]
		public void ParseSourceCode_MultiLineFunctionInSource_CorrectColumnAndZeroBasedLineNumbers()
		{
			// Arrange
			var sourceCode = "function foo()" + Environment.NewLine + "{" + Environment.NewLine + "bar();" +
								Environment.NewLine + "}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Single(functionMap);
			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(9, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(1, functionMap[0].Start.Line);
			Assert.Equal(3, functionMap[0].End.Line);
			Assert.Equal(0, functionMap[0].Start.Column);
			Assert.Equal(1, functionMap[0].End.Column);
		}

		[Fact]
		public void ParseSourceCode_TwoSingleLineFunctions_TwoFunctionMapEntries()
		{
			// Arrange
			var sourceCode = "function foo(){bar();}function bar(){baz();}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("bar", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(31, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(36, functionMap[0].Start.Column);
			Assert.Equal(44, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(9, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(14, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_TwoNestedSingleLineFunctions_TwoFunctionMapEntries()
		{
			// Arrange
			var sourceCode = "function foo(){function bar(){baz();}}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("bar", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(29, functionMap[0].Start.Column);
			Assert.Equal(37, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(9, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(14, functionMap[1].Start.Column);
			Assert.Equal(38, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_FunctionAssignedToVariable_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){bar();}";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Single(functionMap);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(20, functionMap[0].Start.Column);
			Assert.Equal(28, functionMap[0].End.Column);
		}

		[Fact]
		public void ParseSourceCode_StaticMethod_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){};foo.bar = function() { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal("bar", functionMap[0].Bindings[1].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(23, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(44, functionMap[0].Start.Column);
			Assert.Equal(54, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(20, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_InstanceMethod_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype.bar = function () { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal("prototype", functionMap[0].Bindings[1].Name);
			Assert.Equal("bar", functionMap[0].Bindings[2].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(56, functionMap[0].Start.Column);
			Assert.Equal(66, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(20, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_InstanceMethodInObjectInitializer_FunctionMapEntryGenerated()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype = { bar: function () { baz(); } }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal("prototype", functionMap[0].Bindings[1].Name);
			Assert.Equal("bar", functionMap[0].Bindings[2].Name);
			Assert.Equal(0, functionMap[0].Bindings[2].SourcePosition.Line);
			Assert.Equal(42, functionMap[0].Bindings[2].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(59, functionMap[0].Start.Column);
			Assert.Equal(69, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(20, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_FunctionAssignedToVariableAndHasName_FunctionMapEntryGeneratedForVariableName()
		{
			// Arrange
			var sourceCode = "var foo = function myCoolFunctionName(){ bar(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Single(functionMap);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(39, functionMap[0].Start.Column);
			Assert.Equal(49, functionMap[0].End.Column);
		}

		[Fact]
		public void ParseSourceCode_StaticMethodAndFunctionHasName_FunctionMapEntryGeneratedForPropertyName()
		{
			// Arrange
			var sourceCode = "var foo = function(){};foo.bar = function myCoolFunctionName() { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal("bar", functionMap[0].Bindings[1].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(23, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(63, functionMap[0].Start.Column);
			Assert.Equal(73, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(20, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_InstanceMethodAndFunctionHasName_FunctionMapEntryGeneratedForObjectPrototype()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype.bar = function myCoolFunctionName() { baz(); }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal("prototype", functionMap[0].Bindings[1].Name);
			Assert.Equal("bar", functionMap[0].Bindings[2].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(74, functionMap[0].Start.Column);
			Assert.Equal(84, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(20, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
		}

		[Fact]
		public void ParseSourceCode_InstanceMethodWithObjectInitializerAndFunctionHasName_FunctionMapEntryGeneratedForObjectPrototype()
		{
			// Arrange
			var sourceCode = "var foo = function(){}; foo.prototype = { bar: function myCoolFunctionName() { baz(); } }";

			// Act
			var functionMap = FunctionMapGenerator.ParseSourceCode(UnitTestUtils.StreamFromString(sourceCode), CreateSourceMapMock());

			// Assert
			Assert.Equal(2, functionMap.Count);

			Assert.Equal("foo", functionMap[0].Bindings[0].Name);
			Assert.Equal(0, functionMap[0].Bindings[0].SourcePosition.Line);
			Assert.Equal(24, functionMap[0].Bindings[0].SourcePosition.Column);
			Assert.Equal("prototype", functionMap[0].Bindings[1].Name);
			Assert.Equal("bar", functionMap[0].Bindings[2].Name);
			Assert.Equal(0, functionMap[0].Bindings[2].SourcePosition.Line);
			Assert.Equal(42, functionMap[0].Bindings[2].SourcePosition.Column);
			Assert.Equal(0, functionMap[0].Start.Line);
			Assert.Equal(0, functionMap[0].End.Line);
			Assert.Equal(77, functionMap[0].Start.Column);
			Assert.Equal(87, functionMap[0].End.Column);

			Assert.Equal("foo", functionMap[1].Bindings[0].Name);
			Assert.Equal(0, functionMap[1].Bindings[0].SourcePosition.Line);
			Assert.Equal(4, functionMap[1].Bindings[0].SourcePosition.Column);
			Assert.Equal(0, functionMap[1].Start.Line);
			Assert.Equal(0, functionMap[1].End.Line);
			Assert.Equal(20, functionMap[1].Start.Column);
			Assert.Equal(22, functionMap[1].End.Column);
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