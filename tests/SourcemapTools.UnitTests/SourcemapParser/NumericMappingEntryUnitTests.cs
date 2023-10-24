using System.Collections.Generic;
using NUnit.Framework;
using SourcemapTools.SourcemapParser.Internal;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class NumericMappingEntryUnitTests
{
	[Test]
	public void ToMappingEntry_ContainsGeneratedSourcePosition_CorrectMappingEntryFieldsPopulated()
	{
		// Arrange
		var numericMappingEntry = new NumericMappingEntry(13, 12, null, null, null, null);
		var names = new List<string>();
		var sources = new List<string>();

		// Act
		var mappingEntry = numericMappingEntry.ToMappingEntry(names, sources);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(mappingEntry.GeneratedSourcePosition.Column, Is.EqualTo(12));
			Assert.That(mappingEntry.GeneratedSourcePosition.Line, Is.EqualTo(13));
			Assert.That(mappingEntry.OriginalSourcePosition, Is.EqualTo(SourcePosition.NotFound));
		});
		Assert.Multiple(() =>
		{
			Assert.That(mappingEntry.OriginalFileName, Is.Null);
			Assert.That(mappingEntry.OriginalName, Is.Null);
		});
	}

	[Test]
	public void ToMappingEntry_ContainsGeneratedAndOriginalSourcePosition_CorrectMappingEntryFieldsPopulated()
	{
		// Arrange
		var numericMappingEntry = new NumericMappingEntry(3, 2, null, 23, 16, null);
		var names = new List<string>();
		var sources = new List<string>();

		// Act
		var mappingEntry = numericMappingEntry.ToMappingEntry(names, sources);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(mappingEntry.GeneratedSourcePosition.Column, Is.EqualTo(2));
			Assert.That(mappingEntry.GeneratedSourcePosition.Line, Is.EqualTo(3));
			Assert.That(mappingEntry.OriginalSourcePosition.Column, Is.EqualTo(16));
			Assert.That(mappingEntry.OriginalSourcePosition.Line, Is.EqualTo(23));
		});
		Assert.Multiple(() =>
		{
			Assert.That(mappingEntry.OriginalFileName, Is.Null);
			Assert.That(mappingEntry.OriginalName, Is.Null);
		});
	}

	[Test]
	public void ToMappingEntry_ContainsGeneratedPositionNameIndexAndSourcesIndex_CorrectMappingEntryFieldsPopulated()
	{
		// Arrange
		var numericMappingEntry = new NumericMappingEntry(48, 8, 2, null, null, 1);
		var names = new List<string>() { "foo", "bar" };
		var sources = new List<string>() { "one", "two", "three" };

		// Act
		var mappingEntry = numericMappingEntry.ToMappingEntry(names, sources);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(mappingEntry.GeneratedSourcePosition.Column, Is.EqualTo(8));
			Assert.That(mappingEntry.GeneratedSourcePosition.Line, Is.EqualTo(48));
			Assert.That(mappingEntry.OriginalSourcePosition, Is.EqualTo(SourcePosition.NotFound));
			Assert.That(mappingEntry.OriginalFileName, Is.EqualTo("three"));
			Assert.That(mappingEntry.OriginalName, Is.EqualTo("bar"));
		});
	}
}