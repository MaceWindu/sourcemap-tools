using System.Collections.Generic;
using Xunit;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
	public class NumericMappingEntryUnitTests
	{
		[Fact]
		public void ToMappingEntry_ContainsGeneratedSourcePosition_CorrectMappingEntryFieldsPopulated()
		{
			// Arrange
			var numericMappingEntry = new NumericMappingEntry
			{
				GeneratedColumnNumber = 12,
				GeneratedLineNumber = 13
			};
			var names = new List<string>();
			var sources = new List<string>();

			// Act
			var mappingEntry = numericMappingEntry.ToMappingEntry(names, sources);

			// Assert
			Assert.Equal(12, mappingEntry.GeneratedSourcePosition.Column);
			Assert.Equal(13, mappingEntry.GeneratedSourcePosition.Line);
			Assert.Equal(SourcePosition.NotFound, mappingEntry.OriginalSourcePosition);
			Assert.Null(mappingEntry.OriginalFileName);
			Assert.Null(mappingEntry.OriginalName);
		}

		[Fact]
		public void ToMappingEntry_ContainsGeneratedAndOriginalSourcePosition_CorrectMappingEntryFieldsPopulated()
		{
			// Arrange
			var numericMappingEntry = new NumericMappingEntry
			{
				GeneratedColumnNumber = 2,
				GeneratedLineNumber = 3,
				OriginalColumnNumber = 16,
				OriginalLineNumber = 23
			};
			var names = new List<string>();
			var sources = new List<string>();

			// Act
			var mappingEntry = numericMappingEntry.ToMappingEntry(names, sources);

			// Assert
			Assert.Equal(2, mappingEntry.GeneratedSourcePosition.Column);
			Assert.Equal(3, mappingEntry.GeneratedSourcePosition.Line);
			Assert.Equal(16, mappingEntry.OriginalSourcePosition!.Column);
			Assert.Equal(23, mappingEntry.OriginalSourcePosition.Line);
			Assert.Null(mappingEntry.OriginalFileName);
			Assert.Null(mappingEntry.OriginalName);
		}

		[Fact]
		public void ToMappingEntry_ContainsGeneratedPositionNameIndexAndSourcesIndex_CorrectMappingEntryFieldsPopulated()
		{
			// Arrange
			var numericMappingEntry = new NumericMappingEntry
			{
				GeneratedColumnNumber = 8,
				GeneratedLineNumber = 48,
				OriginalNameIndex = 1,
				OriginalSourceFileIndex = 2
			};
			var names = new List<string> {"foo", "bar"};
			var sources = new List<string> { "one", "two", "three"};

			// Act
			var mappingEntry = numericMappingEntry.ToMappingEntry(names, sources);

			// Assert
			Assert.Equal(8, mappingEntry.GeneratedSourcePosition.Column);
			Assert.Equal(48, mappingEntry.GeneratedSourcePosition.Line);
			Assert.Equal(SourcePosition.NotFound, mappingEntry.OriginalSourcePosition);
			Assert.Equal("three", mappingEntry.OriginalFileName);
			Assert.Equal("bar", mappingEntry.OriginalName);
		}
	}
}