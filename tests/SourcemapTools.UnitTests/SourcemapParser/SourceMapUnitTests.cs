using System.Collections.Generic;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class SourceMapUnitTests
{
	[Test]
	public void GetMappingEntryForGeneratedSourcePosition_NullMappingList_ReturnNull()
	{
		// Arrange
		var sourceMap = CreateSourceMap();
		var sourcePosition = new SourcePosition(4, 3);

		// Act
		var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

		// Asset
		Assert.That(result, Is.Null);
	}

	[Test]
	public void GetMappingEntryForGeneratedSourcePosition_NoMatchingEntryInMappingList_ReturnNull()
	{
		// Arrange
		var parsedMappings = new List<MappingEntry>
		{
			new(generatedSourcePosition: new SourcePosition(0, 0))
		};

		var sourceMap = CreateSourceMap(parsedMappings: parsedMappings);
		var sourcePosition = new SourcePosition(4, 3);

		// Act
		var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

		// Asset
		Assert.That(result, Is.Null);
	}

	[Test]
	public void GetMappingEntryForGeneratedSourcePosition_MatchingEntryInMappingList_ReturnMatchingEntry()
	{
		// Arrange
		var matchingMappingEntry = new MappingEntry(
			generatedSourcePosition: new SourcePosition(8, 13));
		var parsedMappings = new List<MappingEntry>
		{
			new(generatedSourcePosition: new SourcePosition(0, 0)),
			matchingMappingEntry
		};

		var sourceMap = CreateSourceMap(parsedMappings: parsedMappings);
		var sourcePosition = new SourcePosition(8, 13);

		// Act
		var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

		// Asset
		Assert.That(result, Is.EqualTo(matchingMappingEntry));
	}

	[Test]
	public void GetMappingEntryForGeneratedSourcePosition_NoExactMatchHasSimilarOnSameLine_ReturnSimilarEntry()
	{
		// Arrange
		var matchingMappingEntry = new MappingEntry(
			generatedSourcePosition: new SourcePosition(10, 13));
		var parsedMappings = new List<MappingEntry>
		{
			new(generatedSourcePosition: new SourcePosition(0, 0)),
			matchingMappingEntry
		};
		var sourceMap = CreateSourceMap(parsedMappings: parsedMappings);
		var sourcePosition = new SourcePosition(10, 14);

		// Act
		var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

		// Asset
		Assert.That(result, Is.EqualTo(matchingMappingEntry));
	}

	[Test]
	public void GetMappingEntryForGeneratedSourcePosition_NoExactMatchHasSimilarOnDifferentLinesLine_ReturnSimilarEntry()
	{
		// Arrange
		var matchingMappingEntry = new MappingEntry(
			generatedSourcePosition: new SourcePosition(23, 15));
		var parsedMappings = new List<MappingEntry>
		{
			new(generatedSourcePosition: new SourcePosition(0, 0)),
			matchingMappingEntry
		};
		var sourceMap = CreateSourceMap(parsedMappings: parsedMappings);
		var sourcePosition = new SourcePosition(24, 0);

		// Act
		var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

		// Asset
		Assert.That(result, Is.EqualTo(matchingMappingEntry));
	}

	[Test]
	public void GetRootMappingEntryForGeneratedSourcePosition_NoChildren_ReturnsSameEntry()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 5);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 5);
		var mappingEntry = UnitTestUtils.GetSimpleEntry(generated1, original1, "generated.js");

		var sourceMap = CreateSourceMap(
			sources: ["generated.js"],
			parsedMappings: [mappingEntry]);

		// Act
		var rootEntry = sourceMap.GetMappingEntryForGeneratedSourcePosition(generated1);

		// Assert
		Assert.That(mappingEntry, Is.EqualTo(rootEntry));
	}

	[Test]
	public void ApplyMap_NoMatchingSources_ReturnsSameMap()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 3);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 2);
		var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "someOtherSource.js");

		var childMap = CreateSourceMap(
			file: "notSourceOne.js",
			sources: ["someOtherSource.js"],
			parsedMappings: [childMapping]);

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 7);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 3);
		var parentMapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

		var parentMap = CreateSourceMap(
			file: "generated.js",
			sources: ["sourceOne.js"],
			parsedMappings: [parentMapping]);

		// Act
		var combinedMap = parentMap.ApplySourceMap(childMap);

		// Assert
		Assert.That(combinedMap, Is.Not.Null);
		Assert.That(combinedMap.ParsedMappings, Is.Not.Null);
		Assert.That(combinedMap.ParsedMappings, Has.Count.GreaterThan(0));
		var firstMapping = combinedMap.ParsedMappings[0];
		Assert.That(firstMapping.IsValueEqual(parentMapping), Is.True);
	}

	[Test]
	public void ApplyMap_NoMatchingMappings_ReturnsSameMap()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 2);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 10);
		var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceTwo.js");

		var childMap = CreateSourceMap(
			file: "sourceOne.js",
			sources: ["sourceTwo.js"],
			parsedMappings: [childMapping]);

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 4);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 5);
		var parentMapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

		var parentMap = CreateSourceMap(
			file: "generated.js",
			sources: ["sourceOne.js"],
			parsedMappings: [parentMapping]);

		// Act
		var combinedMap = parentMap.ApplySourceMap(childMap);

		// Assert
		Assert.That(combinedMap, Is.Not.Null);
		Assert.That(combinedMap.ParsedMappings, Is.Not.Null);
		Assert.That(combinedMap.ParsedMappings, Has.Count.GreaterThan(0));
		var firstMapping = combinedMap.ParsedMappings[0];
		Assert.That(firstMapping.IsValueEqual(parentMapping), Is.True);
	}

	[Test]
	public void ApplyMap_MatchingSources_ReturnsCorrectMap()
	{
		// Expect mapping with same source filename as the applied source-map to be replaced

		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 4);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 3);
		var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceTwo.js");

		var childMap = CreateSourceMap(
			file: "sourceOne.js",
			sources: ["sourceTwo.js"],
			parsedMappings: [childMapping]);

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 4);
		var parentMapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

		var parentMap = CreateSourceMap(
			file: "generated.js",
			sources: ["sourceOne.js"],
			parsedMappings: [parentMapping]);

		// Act
		var combinedMap = parentMap.ApplySourceMap(childMap);

		// Assert
		Assert.That(combinedMap, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(combinedMap.ParsedMappings, Has.Count.EqualTo(1));
			Assert.That(combinedMap.Sources?.Count, Is.EqualTo(1));
		});
		var rootMapping = combinedMap.GetMappingEntryForGeneratedSourcePosition(generated2);
		Assert.That(rootMapping, Is.Not.Null);
		Assert.That(rootMapping.Value.OriginalSourcePosition.CompareTo(childMapping.OriginalSourcePosition), Is.EqualTo(0));
	}

	[Test]
	public void ApplyMap_PartialMatchingSources_ReturnsCorrectMap()
	{
		// Expect mappings with same source filename as the applied source-map to be replaced
		// mappings with a different source filename should stay the same

		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 10);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 5);
		var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceTwo.js");

		var childMap = CreateSourceMap(
			file: "sourceOne.js",
			sources: ["sourceTwo.js"],
			parsedMappings: [childMapping]);

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 2);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 10);
		var mapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

		var generated3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 4, colNumber: 3);
		var original3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 2);
		var mapping2 = UnitTestUtils.GetSimpleEntry(generated3, original3, "noMapForThis.js");

		var parentMap = CreateSourceMap(
			file: "generated.js",
			sources: ["sourceOne.js", "noMapForThis.js"],
			parsedMappings: [mapping, mapping2]);

		// Act
		var combinedMap = parentMap.ApplySourceMap(childMap);

		// Assert
		Assert.That(combinedMap, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(combinedMap.ParsedMappings, Has.Count.EqualTo(2));
			Assert.That(combinedMap.Sources, Has.Count.EqualTo(2));
		});
		var firstCombinedMapping = combinedMap.GetMappingEntryForGeneratedSourcePosition(generated3);
		Assert.That(firstCombinedMapping, Is.Not.Null);
		Assert.That(firstCombinedMapping.Value.IsValueEqual(mapping2), Is.True);
		var secondCombinedMapping = combinedMap.GetMappingEntryForGeneratedSourcePosition(generated2);
		Assert.That(secondCombinedMapping, Is.Not.Null);
		Assert.That(secondCombinedMapping.Value.OriginalSourcePosition.CompareTo(childMapping.OriginalSourcePosition), Is.EqualTo(0));
	}

	[Test]
	public void ApplyMap_ExactMatchDeep_ReturnsCorrectMappingEntry()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 10);
		var mapLevel2 = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceThree.js");

		var grandChildMap = CreateSourceMap(
			file: "sourceTwo.js",
			sources: ["sourceThree.js"],
			parsedMappings: [mapLevel2]);

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 4, colNumber: 3);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
		var mapLevel1 = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceTwo.js");

		var childMap = CreateSourceMap(
			file: "sourceOne.js",
			sources: ["sourceTwo.js"],
			parsedMappings: [mapLevel1]);

		var generated3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 5, colNumber: 5);
		var original3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 4, colNumber: 3);
		var mapLevel0 = UnitTestUtils.GetSimpleEntry(generated3, original3, "sourceOne.js");

		var parentMap = CreateSourceMap(
			file: "generated.js",
			sources: ["sourceOne.js"],
			parsedMappings: [mapLevel0]);

		// Act
		var firstCombinedMap = parentMap.ApplySourceMap(childMap);

		// Assert
		Assert.That(firstCombinedMap, Is.Not.Null);
		var secondCombinedMap = firstCombinedMap.ApplySourceMap(grandChildMap);
		Assert.That(secondCombinedMap, Is.Not.Null);
		var rootMapping = secondCombinedMap.GetMappingEntryForGeneratedSourcePosition(generated3);
		Assert.That(rootMapping, Is.Not.Null);
		Assert.That(rootMapping.Value.OriginalSourcePosition.CompareTo(mapLevel2.OriginalSourcePosition), Is.EqualTo(0));
	}

	private static SourceMap CreateSourceMap(
		int version = default,
		string? file = default,
		string? mappings = default,
		IReadOnlyList<string>? sources = default,
		IReadOnlyList<string>? names = default,
		IReadOnlyList<MappingEntry>? parsedMappings = default,
		IReadOnlyList<string>? sourcesContent = default) => new(
			version: version,
			file: file,
			mappings: mappings,
			sources: sources,
			names: names,
			parsedMappings: parsedMappings ?? [],
			sourcesContent: sourcesContent);
}
