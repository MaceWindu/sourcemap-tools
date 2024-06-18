using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

/// <summary>
/// Summary description for SourceMapTransformerUnitTests
/// </summary>
public class SourceMapTransformerUnitTests
{

	[Test]
	public void FlattenMap_ReturnsOnlyLineInformation()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 2);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 2);
		var mappingEntry = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceOne.js");

		var map = new SourceMap
		{
			File = "generated.js",
			Sources = ["sourceOne.js"],
			ParsedMappings = [mappingEntry],
			SourcesContent = ["var a = b"]
		};

		// Act
		var linesOnlyMap = SourceMapTransformer.Flatten(map);

		// Assert
		Assert.That(linesOnlyMap, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(linesOnlyMap.Sources?.Count, Is.EqualTo(1));
			Assert.That(linesOnlyMap.SourcesContent?.Count, Is.EqualTo(1));
			Assert.That(linesOnlyMap.ParsedMappings, Has.Count.EqualTo(1));
		});
		Assert.Multiple(() =>
		{
			Assert.That(linesOnlyMap.ParsedMappings[0].GeneratedSourcePosition.Line, Is.EqualTo(1));
			Assert.That(linesOnlyMap.ParsedMappings[0].GeneratedSourcePosition.Column, Is.EqualTo(0));
			Assert.That(linesOnlyMap.ParsedMappings[0].OriginalSourcePosition.Line, Is.EqualTo(2));
			Assert.That(linesOnlyMap.ParsedMappings[0].OriginalSourcePosition.Column, Is.EqualTo(0));
		});
	}

	[Test]
	public void FlattenMap_MultipleMappingsSameLine_ReturnsOnlyOneMappingPerLine()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 2);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 2);
		var mappingEntry = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceOne.js");

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 5);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 5);
		var mappingEntry2 = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

		var map = new SourceMap(
			version: default,
			file: "generated.js",
			mappings: default,
			sources: ["sourceOne.js"],
			names: default,
			parsedMappings: [mappingEntry, mappingEntry2],
			sourcesContent: ["var a = b"]);

		// Act
		var linesOnlyMap = SourceMapTransformer.Flatten(map);

		// Assert
		Assert.That(linesOnlyMap, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(linesOnlyMap.Sources?.Count, Is.EqualTo(1));
			Assert.That(linesOnlyMap.SourcesContent?.Count, Is.EqualTo(1));
			Assert.That(linesOnlyMap.ParsedMappings, Has.Count.EqualTo(1));
		});
		Assert.Multiple(() =>
		{
			Assert.That(linesOnlyMap.ParsedMappings[0].GeneratedSourcePosition.Line, Is.EqualTo(1));
			Assert.That(linesOnlyMap.ParsedMappings[0].GeneratedSourcePosition.Column, Is.EqualTo(0));
			Assert.That(linesOnlyMap.ParsedMappings[0].OriginalSourcePosition.Line, Is.EqualTo(2));
			Assert.That(linesOnlyMap.ParsedMappings[0].OriginalSourcePosition.Column, Is.EqualTo(0));
		});
	}

	[Test]
	public void FlattenMap_MultipleOriginalLineToSameGeneratedLine_ReturnsFirstOriginalLine()
	{
		// Arrange
		var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 2);
		var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 2);
		var mappingEntry = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceOne.js");

		var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 3);
		var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
		var mappingEntry2 = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

		var map = new SourceMap(
			version: default,
			file: "generated.js",
			mappings: default,
			sources: ["sourceOne.js"],
			names: default,
			parsedMappings: [mappingEntry, mappingEntry2],
			sourcesContent: ["var a = b"]);

		// Act
		var linesOnlyMap = SourceMapTransformer.Flatten(map);

		// Assert
		Assert.That(linesOnlyMap, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(linesOnlyMap.Sources?.Count, Is.EqualTo(1));
			Assert.That(linesOnlyMap.SourcesContent?.Count, Is.EqualTo(1));
			Assert.That(linesOnlyMap.ParsedMappings, Has.Count.EqualTo(1));
		});
		Assert.Multiple(() =>
		{
			Assert.That(linesOnlyMap.ParsedMappings[0].GeneratedSourcePosition.Line, Is.EqualTo(1));
			Assert.That(linesOnlyMap.ParsedMappings[0].GeneratedSourcePosition.Column, Is.EqualTo(0));
			Assert.That(linesOnlyMap.ParsedMappings[0].OriginalSourcePosition.Line, Is.EqualTo(2));
			Assert.That(linesOnlyMap.ParsedMappings[0].OriginalSourcePosition.Column, Is.EqualTo(0));
		});
	}
}
