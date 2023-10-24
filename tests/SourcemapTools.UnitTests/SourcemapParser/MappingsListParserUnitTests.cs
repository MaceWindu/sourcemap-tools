
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SourcemapTools.SourcemapParser.Internal;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class MappingsListParserUnitTests
{
	[Test]
	public void ParseSingleMappingSegment_0SegmentFields_ArgumentOutOfRangeException()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState();
		var segmentFields = new List<int>();

		// Act
		Assert.Throws<ArgumentOutOfRangeException>(() => MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState));
	}

	[Test]
	public void ParseSingleMappingSegment_2SegmentFields_ArgumentOutOfRangeException()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState();
		var segmentFields = new List<int> { 1, 2 };

		// Act
		Assert.Throws<ArgumentOutOfRangeException>(() => MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState));
	}

	[Test]
	public void ParseSingleMappingSegment_3SegmentFields_ArgumentOutOfRangeException()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState();
		var segmentFields = new List<int> { 1, 2, 3 };

		// Act
		Assert.Throws<ArgumentOutOfRangeException>(() => MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState));
	}

	[Test]
	public void ParseSingleMappingSegment_NoPreviousStateSingleSegment_GeneratedColumnSet()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState();
		var segmentFields = new List<int> { 16 };

		// Act
		var result = MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.GeneratedLineNumber, Is.EqualTo(0));
			Assert.That(result.GeneratedColumnNumber, Is.EqualTo(16));
		});
		Assert.Multiple(() =>
		{
			Assert.That(result.OriginalSourceFileIndex.HasValue, Is.False);
			Assert.That(result.OriginalLineNumber.HasValue, Is.False);
			Assert.That(result.OriginalColumnNumber.HasValue, Is.False);
			Assert.That(result.OriginalNameIndex.HasValue, Is.False);
		});
	}

	[Test]
	public void ParseSingleMappingSegment_NoPreviousState4Segments_OriginalNameIndexNotSetInMappingEntry()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState();
		var segmentFields = new List<int> { 1, 1, 2, 4 };

		// Act
		var result = MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.GeneratedLineNumber, Is.EqualTo(0));
			Assert.That(result.GeneratedColumnNumber, Is.EqualTo(1));
			Assert.That(result.OriginalSourceFileIndex, Is.EqualTo(1));
			Assert.That(result.OriginalLineNumber, Is.EqualTo(2));
			Assert.That(result.OriginalColumnNumber, Is.EqualTo(4));
		});
		Assert.That(result.OriginalNameIndex.HasValue, Is.False);
	}

	[Test]
	public void ParseSingleMappingSegment_NoPreviousState5Segments_AllFieldsSetInMappingEntry()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState();
		var segmentFields = new List<int> { 1, 3, 6, 10, 15 };

		// Act
		var result = MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.GeneratedLineNumber, Is.EqualTo(0));
			Assert.That(result.GeneratedColumnNumber, Is.EqualTo(1));
			Assert.That(result.OriginalSourceFileIndex, Is.EqualTo(3));
			Assert.That(result.OriginalLineNumber, Is.EqualTo(6));
			Assert.That(result.OriginalColumnNumber, Is.EqualTo(10));
			Assert.That(result.OriginalNameIndex, Is.EqualTo(15));
		});
	}

	[Test]
	public void ParseSingleMappingSegment_HasPreviousState5Segments_AllFieldsSetIncrementally()
	{
		// Arrange
		var mappingsParserState = new MappingsParserState(newGeneratedColumnBase: 6,
			newSourcesListIndexBase: 7,
			newOriginalSourceStartingLineBase: 8,
			newOriginalSourceStartingColumnBase: 9,
			newNamesListIndexBase: 10);

		var segmentFields = new List<int> { 1, 2, 3, 4, 5 };

		// Act
		var result = MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result.GeneratedLineNumber, Is.EqualTo(0));
			Assert.That(result.GeneratedColumnNumber, Is.EqualTo(7));
			Assert.That(result.OriginalSourceFileIndex, Is.EqualTo(9));
			Assert.That(result.OriginalLineNumber, Is.EqualTo(11));
			Assert.That(result.OriginalColumnNumber, Is.EqualTo(13));
			Assert.That(result.OriginalNameIndex, Is.EqualTo(15));
		});
	}

	[Test]
	public void ParseMappings_SingleSemicolon_GeneratedLineNumberNotIncremented()
	{
		// Arrange
		var mappingsString = "ktC,2iB;";
		var names = new List<string>();
		var sources = new List<string>();

		// Act
		var mappingsList = MappingsListParser.ParseMappings(mappingsString, names, sources);

		// Assert
		Assert.That(mappingsList, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(mappingsList[0].GeneratedSourcePosition.Line, Is.EqualTo(0));
			Assert.That(mappingsList[1].GeneratedSourcePosition.Line, Is.EqualTo(0));
		});
	}

	[Test]
	public void ParseMappings_TwoSemicolons_GeneratedLineNumberIncremented()
	{
		// Arrange
		var mappingsString = "ktC;2iB;";
		var names = new List<string>();
		var sources = new List<string>();

		// Act
		var mappingsList = MappingsListParser.ParseMappings(mappingsString, names, sources);

		// Assert
		Assert.That(mappingsList, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(mappingsList[0].GeneratedSourcePosition.Line, Is.EqualTo(0));
			Assert.That(mappingsList[1].GeneratedSourcePosition.Line, Is.EqualTo(1));
		});
	}

	[Test]
	public void ParseMappings_BackToBackSemiColons_GeneratedLineNumberIncremented()
	{
		// Arrange
		var mappingsString = "ktC;;2iB";
		var names = new List<string>();
		var sources = new List<string>();

		// Act
		var mappingsList = MappingsListParser.ParseMappings(mappingsString, names, sources);

		// Assert
		Assert.That(mappingsList, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(mappingsList[0].GeneratedSourcePosition.Line, Is.EqualTo(0));
			Assert.That(mappingsList[1].GeneratedSourcePosition.Line, Is.EqualTo(2));
		});
	}
}