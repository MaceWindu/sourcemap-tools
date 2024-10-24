﻿using System.Collections.Generic;
using NUnit.Framework;
using SourcemapToolkit.SourcemapParser;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class FunctionMapConsumerUnitTests
{
	[Test]
	public void GetWrappingFunctionForSourceLocation_EmptyFunctionMap_ReturnNull()
	{
		// Arrange
		var sourcePosition = new SourcePosition(2, 3);
		var functionMap = new List<FunctionMapEntry>();
		IFunctionMapConsumer functionMapConsumer = new FunctionMapConsumer();

		// Act
		var wrappingFunction = functionMapConsumer.GetWrappingFunctionForSourceLocation(sourcePosition, functionMap);

		// Assert
		Assert.That(wrappingFunction, Is.Null);
	}

	[Test]
	public void GetWrappingFunctionForSourceLocation_SingleIrrelevantFunctionMapEntry_ReturnNull()
	{
		// Arrange
		var sourcePosition = new SourcePosition(2, 3);
		var functionMap = new List<FunctionMapEntry>
		{
			new(
				[],
				null,
				new SourcePosition(40, 10),
				new SourcePosition(50, 10))
		};
		IFunctionMapConsumer functionMapConsumer = new FunctionMapConsumer();

		// Act
		var wrappingFunction = functionMapConsumer.GetWrappingFunctionForSourceLocation(sourcePosition, functionMap);

		// Assert
		Assert.That(wrappingFunction, Is.Null);
	}

	[Test]
	public void GetWrappingFunctionForSourceLocation_SingleRelevantFunctionMapEntry_ReturnWrappingFunction()
	{
		// Arrange
		var sourcePosition = new SourcePosition(41, 2);
		var functionMapEntry = new FunctionMapEntry(
			[],
			null,
			new SourcePosition(40, 10),
			new SourcePosition(50, 10));

		var functionMap = new List<FunctionMapEntry>
		{
			functionMapEntry
		};
		IFunctionMapConsumer functionMapConsumer = new FunctionMapConsumer();

		// Act
		var wrappingFunction = functionMapConsumer.GetWrappingFunctionForSourceLocation(sourcePosition, functionMap);

		// Assert
		Assert.That(wrappingFunction, Is.EqualTo(functionMapEntry));
	}

	[Test]
	public void GetWrappingFunctionForSourceLocation_MultipleFunctionMapEntriesSingleRelevantFunctionMapEntry_ReturnWrappingFunction()
	{
		// Arrange
		var sourcePosition = new SourcePosition(31, 0);
		var functionMapEntry = new FunctionMapEntry(
			[],
			null,
			new SourcePosition(10, 10),
			new SourcePosition(20, 30));

		var functionMapEntry2 = new FunctionMapEntry(
			[],
			null,
			new SourcePosition(30, 0),
			new SourcePosition(40, 2));

		var functionMap = new List<FunctionMapEntry>
		{
			functionMapEntry,
			functionMapEntry2,
		};
		IFunctionMapConsumer functionMapConsumer = new FunctionMapConsumer();

		// Act
		var wrappingFunction = functionMapConsumer.GetWrappingFunctionForSourceLocation(sourcePosition, functionMap);

		// Assert
		Assert.That(wrappingFunction, Is.EqualTo(functionMapEntry2));
	}

	[Test]
	public void GetWrappingFunctionForSourceLocation_MultipleFunctionMapEntriesMultipleRelevantFunctionMapEntry_ReturnClosestWrappingFunction()
	{
		// Arrange
		var sourcePosition = new SourcePosition(10, 25);
		var functionMapEntry = new FunctionMapEntry(
			[],
			null,
			new SourcePosition(5, 10),
			new SourcePosition(20, 30));

		var functionMapEntry2 = new FunctionMapEntry(
			[],
			null,
			new SourcePosition(9, 0),
			new SourcePosition(15, 2));

		var functionMap = new List<FunctionMapEntry>
		{
			functionMapEntry2,
			functionMapEntry
		};
		IFunctionMapConsumer functionMapConsumer = new FunctionMapConsumer();

		// Act
		var wrappingFunction = functionMapConsumer.GetWrappingFunctionForSourceLocation(sourcePosition, functionMap);

		// Assert
		Assert.That(wrappingFunction, Is.EqualTo(functionMapEntry2));
	}
}
