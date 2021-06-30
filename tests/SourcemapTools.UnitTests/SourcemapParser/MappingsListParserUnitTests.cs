
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{

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

			// Assert
			Assert.AreEqual(0, result.GeneratedLineNumber);
			Assert.AreEqual(16, result.GeneratedColumnNumber);
			Assert.False(result.OriginalSourceFileIndex.HasValue);
			Assert.False(result.OriginalLineNumber.HasValue);
			Assert.False(result.OriginalColumnNumber.HasValue);
			Assert.False(result.OriginalNameIndex.HasValue);
		}

		[Test]
		public void ParseSingleMappingSegment_NoPreviousState4Segments_OriginalNameIndexNotSetInMappingEntry()
		{
			// Arrange
			var mappingsParserState = new MappingsParserState();
			var segmentFields = new List<int> { 1, 1, 2, 4 };

			// Act 
			var result = MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState);

			// Assert
			Assert.AreEqual(0, result.GeneratedLineNumber);
			Assert.AreEqual(1, result.GeneratedColumnNumber);
			Assert.AreEqual(1, result.OriginalSourceFileIndex);
			Assert.AreEqual(2, result.OriginalLineNumber);
			Assert.AreEqual(4, result.OriginalColumnNumber);
			Assert.False(result.OriginalNameIndex.HasValue);
		}

		[Test]
		public void ParseSingleMappingSegment_NoPreviousState5Segments_AllFieldsSetInMappingEntry()
		{
			// Arrange
			var mappingsParserState = new MappingsParserState();
			var segmentFields = new List<int> { 1, 3, 6, 10, 15 };

			// Act
			var result = MappingsListParser.ParseSingleMappingSegment(segmentFields, mappingsParserState);

			// Assert
			Assert.AreEqual(0, result.GeneratedLineNumber);
			Assert.AreEqual(1, result.GeneratedColumnNumber);
			Assert.AreEqual(3, result.OriginalSourceFileIndex);
			Assert.AreEqual(6, result.OriginalLineNumber);
			Assert.AreEqual(10, result.OriginalColumnNumber);
			Assert.AreEqual(15, result.OriginalNameIndex);
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

			// Assert
			Assert.AreEqual(0, result.GeneratedLineNumber);
			Assert.AreEqual(7, result.GeneratedColumnNumber);
			Assert.AreEqual(9, result.OriginalSourceFileIndex);
			Assert.AreEqual(11, result.OriginalLineNumber);
			Assert.AreEqual(13, result.OriginalColumnNumber);
			Assert.AreEqual(15, result.OriginalNameIndex);
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
			Assert.AreEqual(2, mappingsList.Count);
			Assert.AreEqual(0, mappingsList[0].GeneratedSourcePosition.Line);
			Assert.AreEqual(0, mappingsList[1].GeneratedSourcePosition.Line);
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
			Assert.AreEqual(2, mappingsList.Count);
			Assert.AreEqual(0, mappingsList[0].GeneratedSourcePosition.Line);
			Assert.AreEqual(1, mappingsList[1].GeneratedSourcePosition.Line);
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
			Assert.AreEqual(2, mappingsList.Count);
			Assert.AreEqual(0, mappingsList[0].GeneratedSourcePosition.Line);
			Assert.AreEqual(2, mappingsList[1].GeneratedSourcePosition.Line);
		}
	}
}