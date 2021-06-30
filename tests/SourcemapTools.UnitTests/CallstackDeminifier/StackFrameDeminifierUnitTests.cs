using System;
using System.Collections.Generic;
using Moq;
using SourcemapToolkit.SourcemapParser;
using NUnit.Framework;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class StackFrameDeminifierUnitTests
	{
		private static IStackFrameDeminifier GetStackFrameDeminifierWithMockDependencies(ISourceMapStore? sourceMapStore = null, IFunctionMapStore? functionMapStore = null, IFunctionMapConsumer? functionMapConsumer = null, bool useSimpleStackFrameDeminier = false)
		{
			if (sourceMapStore == null)
			{
				sourceMapStore = new Mock<ISourceMapStore>().Object;
			}

			if (functionMapStore == null)
			{
				functionMapStore = new Mock<IFunctionMapStore>().Object;
			}

			if (functionMapConsumer == null)
			{
				functionMapConsumer = new Mock<IFunctionMapConsumer>().Object;
			}

			if (useSimpleStackFrameDeminier)
			{
				return new MethodNameStackFrameDeminifier(functionMapStore, functionMapConsumer);
			}
			else
			{
				return new StackFrameDeminifier(sourceMapStore, functionMapStore, functionMapConsumer);
			}
		}

		[Test]
		public void DeminifyStackFrame_StackFrameNullProperties_DoesNotThrowException()
		{
			// Arrange
			var stackFrame = new StackFrame(null);
			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies();

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Test]
		public void SimpleStackFrameDeminierDeminifyStackFrame_FunctionMapReturnsNull_NoFunctionMapDeminificationError()
		{
			// Arrange
			var filePath = "foo";
			var stackFrame = new StackFrame(null) {FilePath = filePath };
			var functionMapStore = new Mock<IFunctionMapStore>();
			functionMapStore.Setup(c => c.GetFunctionMapForSourceCode(filePath))
				.Returns<FunctionMapEntry>(null);

			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore.Object, useSimpleStackFrameDeminier:true);

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.AreEqual(DeminificationError.NoSourceCodeProvided, stackFrameDeminification.DeminificationError);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Test]
		public void SimpleStackFrameDeminierDeminifyStackFrame_GetWRappingFunctionForSourceLocationReturnsNull_NoWrapingFunctionDeminificationError()
		{
			// Arrange
			var filePath = "foo";
			var stackFrame = new StackFrame(null) { FilePath = filePath };
			var functionMapStore = new Mock<IFunctionMapStore>();
			functionMapStore.Setup(c => c.GetFunctionMapForSourceCode(filePath))
				.Returns(new List<FunctionMapEntry>());
			var functionMapConsumer = new Mock<IFunctionMapConsumer>();
			functionMapConsumer.Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
				.Returns<FunctionMapEntry?>(null);

			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore.Object, functionMapConsumer: functionMapConsumer.Object, useSimpleStackFrameDeminier: true);

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.AreEqual(DeminificationError.NoWrapingFunctionFound, stackFrameDeminification.DeminificationError);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Test]
		public void SimpleStackFrameDeminierDeminifyStackFrame_WrapingFunctionFound_NoDeminificationError()
		{
			// Arrange
			var filePath = "foo";
			var wrapingFunctionMapEntry = CreateFunctionMapEntry("DeminifiedFoo");
			var stackFrame = new StackFrame(null) { FilePath = filePath };
			var functionMapStore = new Mock<IFunctionMapStore>();
			functionMapStore.Setup(c => c.GetFunctionMapForSourceCode(filePath))
				.Returns(new List<FunctionMapEntry>());
			var functionMapConsumer = new Mock<IFunctionMapConsumer>();
			functionMapConsumer.Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
				.Returns(wrapingFunctionMapEntry);

			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore.Object, functionMapConsumer: functionMapConsumer.Object, useSimpleStackFrameDeminier: true);

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.AreEqual(DeminificationError.None, stackFrameDeminification.DeminificationError);
			Assert.AreEqual(wrapingFunctionMapEntry.DeminifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}


		[Test]
		public void StackFrameDeminierDeminifyStackFrame_SourceMapProviderReturnsNull_NoSourcemapProvidedError()
		{
			// Arrange
			var filePath = "foo";
			var wrapingFunctionMapEntry = CreateFunctionMapEntry(deminifiedMethodName: "DeminifiedFoo");
			var stackFrame = new StackFrame(null) { FilePath = filePath };
			var functionMapStore = new Mock<IFunctionMapStore>();
			functionMapStore.Setup(c => c.GetFunctionMapForSourceCode(filePath))
				.Returns(new List<FunctionMapEntry>());
			var functionMapConsumer = new Mock<IFunctionMapConsumer>();
			functionMapConsumer.Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
				.Returns(wrapingFunctionMapEntry);

			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore.Object, functionMapConsumer: functionMapConsumer.Object);

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.AreEqual(DeminificationError.NoSourceMap, stackFrameDeminification.DeminificationError);
			Assert.AreEqual(wrapingFunctionMapEntry.DeminifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Test]
		public void StackFrameDeminierDeminifyStackFrame_SourceMapParsingNull_SourceMapFailedToParseError()
		{
			// Arrange
			var filePath = "foo";
			var wrapingFunctionMapEntry = CreateFunctionMapEntry(deminifiedMethodName: "DeminifiedFoo");
			var stackFrame = new StackFrame(null) { FilePath = filePath };
			var functionMapStore = new Mock<IFunctionMapStore>();
			functionMapStore.Setup(c => c.GetFunctionMapForSourceCode(filePath))
				.Returns(new List<FunctionMapEntry>());
			var functionMapConsumer = new Mock<IFunctionMapConsumer>();
			functionMapConsumer.Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
				.Returns(wrapingFunctionMapEntry);
			var sourceMapStore = new Mock<ISourceMapStore>();
			sourceMapStore.Setup(c => c.GetSourceMapForUrl(It.IsAny<string>())).Returns(CreateSourceMap());

			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore.Object, functionMapStore: functionMapStore.Object, functionMapConsumer: functionMapConsumer.Object);

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.AreEqual(DeminificationError.NoMatchingMapingInSourceMap, stackFrameDeminification.DeminificationError);
			Assert.AreEqual(wrapingFunctionMapEntry.DeminifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Test]
		public void StackFrameDeminierDeminifyStackFrame_SourceMapGeneratedMappingEntryNull_NoMatchingMapingInSourceMapError()
		{
			// Arrange
			var filePath = "foo";
			var wrapingFunctionMapEntry = CreateFunctionMapEntry(deminifiedMethodName: "DeminifiedFoo");
			var stackFrame = new StackFrame(null) { FilePath = filePath };
			var functionMapStore = new Mock<IFunctionMapStore>();
			functionMapStore.Setup(c => c.GetFunctionMapForSourceCode(filePath))
				.Returns(new List<FunctionMapEntry>());
			var sourceMapStore = new Mock<ISourceMapStore>();
			var sourceMap = CreateSourceMap(new List<MappingEntry>());

			sourceMapStore.Setup(c => c.GetSourceMapForUrl(It.IsAny<string>())).Returns(sourceMap);
			var functionMapConsumer = new Mock<IFunctionMapConsumer>();
			functionMapConsumer.Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
				.Returns(wrapingFunctionMapEntry);

			var stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore.Object, functionMapStore: functionMapStore.Object, functionMapConsumer: functionMapConsumer.Object);

			// Act
			var stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName: null);

			// Assert
			Assert.AreEqual(DeminificationError.NoMatchingMapingInSourceMap, stackFrameDeminification.DeminificationError);
			Assert.AreEqual(wrapingFunctionMapEntry.DeminifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.AreEqual(SourcePosition.NotFound, stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}


		private static FunctionMapEntry CreateFunctionMapEntry(string deminifiedMethodName)
		{
			return new FunctionMapEntry(
				default!,
				deminifiedMethodName,
				default,
				default);
		}

		private static SourceMap CreateSourceMap(List<MappingEntry>? parsedMappings = null)
		{
			return new SourceMap(
				default,
				default,
				default,
				default,
				default,
				parsedMappings ?? new List<MappingEntry>(),
				default);
		}
	}
}
