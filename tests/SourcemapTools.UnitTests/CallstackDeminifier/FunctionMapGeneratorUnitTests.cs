using System;
using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;
using SourcemapToolkit.SourcemapParser.UnitTests;
using NUnit.Framework;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class FunctionMapGeneratorUnitTests
{
	[Test]
	public void GenerateFunctionMap_NullSourceMap_ReturnsNull()
	{
		// Arrange
		IFunctionMapGenerator functionMapGenerator = new FunctionMapGenerator();
		var sourceCode = "";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = functionMapGenerator.GenerateFunctionMap(stream, null);

		// Assert
		Assert.That(functionMap, Is.Null);
	}

	[Test]
	public void ParseSourceCode_NoFunctionsInSource_EmptyFunctionList()
	{
		// Arrange
		var sourceCode = "bar();";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Is.Empty);
	}

	[Test]
	public void ParseSourceCode_SingleLineFunctionInSource_CorrectZeroBasedColumnNumbers()
	{
		// Arrange
		var sourceCode = "function foo(){bar();}";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(9));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(14));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_MultiLineFunctionInSource_CorrectColumnAndZeroBasedLineNumbers()
	{
		// Arrange
		var sourceCode = "function foo()" + Environment.NewLine + "{" + Environment.NewLine + "bar();" + Environment.NewLine + "}";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(9));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(1));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(3));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(1));
		});
	}

	[Test]
	public void ParseSourceCode_TwoSingleLineFunctions_TwoFunctionMapEntries()
	{
		// Arrange
		var sourceCode = "function foo(){bar();}function bar(){baz();}";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(31));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(36));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(44));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(9));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(14));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_TwoNestedSingleLineFunctions_TwoFunctionMapEntries()
	{
		// Arrange
		var sourceCode = "function foo(){function bar(){baz();}}";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(24));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(29));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(37));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(9));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(14));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(38));
		});
	}

	[Test]
	public void ParseSourceCode_FunctionAssignedToVariable_FunctionMapEntryGenerated()
	{
		// Arrange
		var sourceCode = "var foo = function(){bar();}";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(1));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(28));
		});
	}

	[Test]
	public void ParseSourceCode_StaticMethod_FunctionMapEntryGenerated()
	{
		// Arrange
		var sourceCode = "var foo = function(){};foo.bar = function() { baz(); }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[1].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(23));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(44));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(54));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_InstanceMethod_FunctionMapEntryGenerated()
	{
		// Arrange
		var sourceCode = "var foo = function(){}; foo.prototype.bar = function () { baz(); }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[1].Name, Is.EqualTo("prototype"));
			Assert.That(functionMap[0].Bindings[2].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(24));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(56));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(66));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_InstanceMethodInObjectInitializer_FunctionMapEntryGenerated()
	{
		// Arrange
		var sourceCode = "var foo = function(){}; foo.prototype = { bar: function () { baz(); } }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(24));
			Assert.That(functionMap[0].Bindings[1].Name, Is.EqualTo("prototype"));
			Assert.That(functionMap[0].Bindings[2].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[2].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[2].SourcePosition.Column, Is.EqualTo(42));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(59));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(69));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_FunctionAssignedToVariableAndHasName_FunctionMapEntryGeneratedForVariableName()
	{
		// Arrange
		var sourceCode = "var foo = function myCoolFunctionName(){ bar(); }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(1));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(39));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(49));
		});
	}

	[Test]
	public void ParseSourceCode_StaticMethodAndFunctionHasName_FunctionMapEntryGeneratedForPropertyName()
	{
		// Arrange
		var sourceCode = "var foo = function(){};foo.bar = function myCoolFunctionName() { baz(); }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[1].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(23));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(63));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(73));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_InstanceMethodAndFunctionHasName_FunctionMapEntryGeneratedForObjectPrototype()
	{
		// Arrange
		var sourceCode = "var foo = function(){}; foo.prototype.bar = function myCoolFunctionName() { baz(); }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[1].Name, Is.EqualTo("prototype"));
			Assert.That(functionMap[0].Bindings[2].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(24));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(74));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(84));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	[Test]
	public void ParseSourceCode_InstanceMethodWithObjectInitializerAndFunctionHasName_FunctionMapEntryGeneratedForObjectPrototype()
	{
		// Arrange
		var sourceCode = "var foo = function(){}; foo.prototype = { bar: function myCoolFunctionName() { baz(); } }";
		using var stream = UnitTestUtils.StreamFromString(sourceCode);

		// Act
		var functionMap = FunctionMapGenerator.ParseSourceCode(stream, CreateSourceMapMock());

		// Assert
		Assert.That(functionMap, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(functionMap[0].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[0].SourcePosition.Column, Is.EqualTo(24));
			Assert.That(functionMap[0].Bindings[1].Name, Is.EqualTo("prototype"));
			Assert.That(functionMap[0].Bindings[2].Name, Is.EqualTo("bar"));
			Assert.That(functionMap[0].Bindings[2].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Bindings[2].SourcePosition.Column, Is.EqualTo(42));
			Assert.That(functionMap[0].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[0].Start.Column, Is.EqualTo(77));
			Assert.That(functionMap[0].End.Column, Is.EqualTo(87));

			Assert.That(functionMap[1].Bindings[0].Name, Is.EqualTo("foo"));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Bindings[0].SourcePosition.Column, Is.EqualTo(4));
			Assert.That(functionMap[1].Start.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].End.Line, Is.EqualTo(0));
			Assert.That(functionMap[1].Start.Column, Is.EqualTo(20));
			Assert.That(functionMap[1].End.Column, Is.EqualTo(22));
		});
	}

	private static SourceMap CreateSourceMapMock() => new(
			0 /* version */,
			default /* file */,
			default /* mappings */,
			default /* sources */,
			default /* names */,
			new List<MappingEntry>() /* parsedMappings */,
			default /* sourcesContent */);
}