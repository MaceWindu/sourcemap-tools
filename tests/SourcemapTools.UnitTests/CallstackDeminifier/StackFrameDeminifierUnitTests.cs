using System.Collections.Generic;
using NUnit.Framework;
using SourcemapToolkit.SourcemapParser;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class StackFrameDeminifierUnitTests
{
	private static IStackFrameDeminifier GetStackFrameDeminifierWithMockDependencies(ISourceMapStore? sourceMapStore = null, IFunctionMapStore? functionMapStore = null, IFunctionMapConsumer? functionMapConsumer = null, bool useSimpleStackFrameDeminifier = false)
	{
		sourceMapStore ??= new ISourceMapStoreMock(_ => null);

		functionMapStore ??= new IFunctionMapStoreMock(_ => null);

		functionMapConsumer ??= new IFunctionMapConsumerMock((x, y) => null);

		return useSimpleStackFrameDeminifier
			? new MethodNameStackFrameDeminifier(functionMapStore, functionMapConsumer)
			: new StackFrameDeminifier(sourceMapStore, functionMapStore, functionMapConsumer);
	}

	[Test]
	public void DeminifyStackFrame_StackFrameNullProperties_DoesNotThrowException([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var stackFrame = new StackFrame(null);
		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies();

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.Null);
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
		});
	}

	[Test]
	public void SimpleStackFrameDeminifierDeminifyStackFrame_FunctionMapReturnsNull_NoFunctionMapDeminificationError([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var filePath = "foo";
		var stackFrame = new StackFrame(null) { FilePath = filePath };
		var functionMapStore = new IFunctionMapStoreMock(_ => null);

		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, useSimpleStackFrameDeminifier: true);

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminificationError, Is.EqualTo(DeminificationError.NoSourceCodeProvided));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.Null);
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
		});
	}

	[Test]
	public void SimpleStackFrameDeminifierDeminifyStackFrame_GetWRappingFunctionForSourceLocationReturnsNull_NoWrappingFunctionDeminificationError([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var filePath = "foo";
		var stackFrame = new StackFrame(null) { FilePath = filePath };
		var functionMapStore = new IFunctionMapStoreMock(x => x == filePath ? new List<FunctionMapEntry>() : null);
		var functionMapConsumer = new IFunctionMapConsumerMock((x, y) => null);

		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer, useSimpleStackFrameDeminifier: true);

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminificationError, Is.EqualTo(DeminificationError.NoWrappingFunctionFound));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.Null);
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
		});
	}

	[Test]
	public void SimpleStackFrameDeminifierDeminifyStackFrame_WrappingFunctionFound_NoDeminificationError([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var filePath = "foo";
		var wrappingFunctionMapEntry = CreateFunctionMapEntry("DeminifiedFoo");
		var stackFrame = new StackFrame(null) { FilePath = filePath };
		var functionMapStore = new IFunctionMapStoreMock(x => x == filePath ? new List<FunctionMapEntry>() : null);
		var functionMapConsumer = new IFunctionMapConsumerMock((x, y) => wrappingFunctionMapEntry);

		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer, useSimpleStackFrameDeminifier: true);

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminificationError, Is.EqualTo(DeminificationError.None));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.EqualTo(wrappingFunctionMapEntry.DeminifiedMethodName));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
		});
		Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
	}

	[Test]
	public void StackFrameDeminifierDeminifyStackFrame_SourceMapProviderReturnsNull_NoSourcemapProvidedError([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var filePath = "foo";
		var wrappingFunctionMapEntry = CreateFunctionMapEntry(deminifiedMethodName: "DeminifiedFoo");
		var stackFrame = new StackFrame(null) { FilePath = filePath };
		var functionMapStore = new IFunctionMapStoreMock(x => x == filePath ? new List<FunctionMapEntry>() : null);
		var functionMapConsumer = new IFunctionMapConsumerMock((x, y) => wrappingFunctionMapEntry);

		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminificationError, Is.EqualTo(DeminificationError.NoSourceMap));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.EqualTo(wrappingFunctionMapEntry.DeminifiedMethodName));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
		});
		Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
	}

	[Test]
	public void StackFrameDeminifierDeminifyStackFrame_SourceMapParsingNull_SourceMapFailedToParseError([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var filePath = "foo";
		var wrappingFunctionMapEntry = CreateFunctionMapEntry(deminifiedMethodName: "DeminifiedFoo");
		var stackFrame = new StackFrame(null) { FilePath = filePath };
		var functionMapStore = new IFunctionMapStoreMock(x => x == filePath ? new List<FunctionMapEntry>() : null);
		var functionMapConsumer = new IFunctionMapConsumerMock((x, y) => wrappingFunctionMapEntry);
		var sourceMapStore = new ISourceMapStoreMock(_ => CreateSourceMap());

		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore, functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminificationError, Is.EqualTo(DeminificationError.NoMatchingMappingInSourceMap));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.EqualTo(wrappingFunctionMapEntry.DeminifiedMethodName));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
		});
		Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
	}

	[Test]
	public void StackFrameDeminifierDeminifyStackFrame_SourceMapGeneratedMappingEntryNull_NoMatchingMappingInSourceMapError([Values] bool preferSourceMapsSymbols)
	{
		// Arrange
		var filePath = "foo";
		var wrappingFunctionMapEntry = CreateFunctionMapEntry(deminifiedMethodName: "DeminifiedFoo");
		var stackFrame = new StackFrame(null) { FilePath = filePath };
		var functionMapStore = new IFunctionMapStoreMock(x => x == filePath ? new List<FunctionMapEntry>() : null);
		var sourceMap = CreateSourceMap([]);
		var sourceMapStore = new ISourceMapStoreMock(_ => sourceMap);

		var functionMapConsumer = new IFunctionMapConsumerMock((x, y) => wrappingFunctionMapEntry);

		var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore, functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

		// Act
		var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, null, preferSourceMapsSymbols);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(stackFrameDeminification.DeminificationError, Is.EqualTo(DeminificationError.NoMatchingMappingInSourceMap));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.MethodName, Is.EqualTo(wrappingFunctionMapEntry.DeminifiedMethodName));
			Assert.That(stackFrameDeminification.DeminifiedStackFrame.SourcePosition, Is.EqualTo(SourcePosition.NotFound));
		});
		Assert.That(stackFrameDeminification.DeminifiedStackFrame.FilePath, Is.Null);
	}

	private static FunctionMapEntry CreateFunctionMapEntry(string deminifiedMethodName) => new(
		[],
		deminifiedMethodName,
		default,
		default);

	private static SourceMap CreateSourceMap(List<MappingEntry>? parsedMappings = null) => new(
		default,
		default,
		default,
		default,
		default,
		parsedMappings ?? [],
		default);
}
