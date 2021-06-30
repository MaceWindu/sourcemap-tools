using System.Collections.Generic;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
	public class NumericMappingEntryUnitTests
	{
		[Test]
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
			Assert.AreEqual(12, mappingEntry.GeneratedSourcePosition.Column);
			Assert.AreEqual(13, mappingEntry.GeneratedSourcePosition.Line);
			Assert.AreEqual(SourcePosition.NotFound, mappingEntry.OriginalSourcePosition);
			Assert.Null(mappingEntry.OriginalFileName);
			Assert.Null(mappingEntry.OriginalName);
		}

		[Test]
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
			Assert.AreEqual(2, mappingEntry.GeneratedSourcePosition.Column);
			Assert.AreEqual(3, mappingEntry.GeneratedSourcePosition.Line);
			Assert.AreEqual(16, mappingEntry.OriginalSourcePosition!.Column);
			Assert.AreEqual(23, mappingEntry.OriginalSourcePosition.Line);
			Assert.Null(mappingEntry.OriginalFileName);
			Assert.Null(mappingEntry.OriginalName);
		}

		[Test]
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
			Assert.AreEqual(8, mappingEntry.GeneratedSourcePosition.Column);
			Assert.AreEqual(48, mappingEntry.GeneratedSourcePosition.Line);
			Assert.AreEqual(SourcePosition.NotFound, mappingEntry.OriginalSourcePosition);
			Assert.AreEqual("three", mappingEntry.OriginalFileName);
			Assert.AreEqual("bar", mappingEntry.OriginalName);
		}
	}
}