using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SourcemapTools.SourcemapParser.Internal;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class SourceMapGeneratorUnitTests
{
	[Test]
	public void SerializeMappingEntry_DifferentLineNumber_SemicolonAdded()
	{
		// Arrange
		var state = new MappingGenerateState(["Name"], ["Source"]);
		state.UpdateLastGeneratedPositionColumn(1);

		var entry = new MappingEntry(
			new SourcePosition(1, 0),
			new SourcePosition(1, 0),
			state.Names[0],
			state.Sources[0]);

		// Act
		var result = new StringBuilder();
		SourceMapGenerator.SerializeMappingEntry(entry, state, result);

		// Assert
		Assert.That(result.ToString(), Does.Contain(";"));
	}

	[Test]
	public void SerializeMappingEntry_NoOriginalFileName_OneSegment()
	{
		// Arrange
		var state = new MappingGenerateState(["Name"], ["Source"]);

		var entry = new MappingEntry(
			new SourcePosition(0, 10),
			new SourcePosition(0, 1),
			null, null);

		// Act
		var result = new StringBuilder();
		SourceMapGenerator.SerializeMappingEntry(entry, state, result);

		// Assert
		Assert.That(result.ToString(), Is.EqualTo("U"));
	}

	[Test]
	public void SerializeMappingEntry_WithOriginalFileNameNoOriginalName_FourSegments()
	{
		// Arrange
		var state = new MappingGenerateState(["Name"], ["Source"])
		{
			IsFirstSegment = false
		};

		var entry = new MappingEntry(
			new SourcePosition(0, 10),
			new SourcePosition(0, 5),
			null,
			state.Sources[0]);

		// Act
		var result = new StringBuilder();
		SourceMapGenerator.SerializeMappingEntry(entry, state, result);

		// Assert
		Assert.That(result.ToString(), Is.EqualTo(",UAAK"));
	}

	[Test]
	public void SerializeMappingEntry_WithOriginalFileNameAndOriginalName_FiveSegments()
	{
		// Arrange
		var state = new MappingGenerateState(["Name"], ["Source"]);
		state.AdvanceLastGeneratedPositionLine();

		var entry = new MappingEntry(
			new SourcePosition(1, 5),
			new SourcePosition(1, 6),
			state.Names[0],
			state.Sources[0]);

		// Act
		var result = new StringBuilder();
		SourceMapGenerator.SerializeMappingEntry(entry, state, result);

		// Assert
		Assert.That(result.ToString(), Is.EqualTo("KACMA"));
	}

	[Test]
	public void SerializeMapping_SimpleSourceMap_CorrectlySerialized()
	{
		// Arrange
		var input = GetSimpleSourceMap();

		// Act
		var output = SourceMapGenerator.SerializeMapping(input);

		// Assert
		Assert.That(/*lang=json,strict*/ output, Is.EqualTo(/*lang=json,strict*/ "{\"version\":3,\"file\":\"CommonIntl\",\"mappings\":\"AACAA,aAAA,CAAc;\",\"sources\":[\"input/CommonIntl.js\"],\"names\":[\"CommonStrings\",\"afrikaans\"]}"));
	}

	[Test]
	public void SerializeMappingBase64_SimpleSourceMap_CorrectlySerialized()
	{
		// Arrange
		var input = GetSimpleSourceMap();

		// Act
		var output = SourceMapGenerator.GenerateSourceMapInlineComment(input);

		// Assert
		Assert.That(output, Is.EqualTo("//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiQ29tbW9uSW50bCIsIm1hcHBpbmdzIjoiQUFDQUEsYUFBQSxDQUFjOyIsInNvdXJjZXMiOlsiaW5wdXQvQ29tbW9uSW50bC5qcyJdLCJuYW1lcyI6WyJDb21tb25TdHJpbmdzIiwiYWZyaWthYW5zIl19"));
	}

	private static SourceMap GetSimpleSourceMap()
	{
		var sources = new List<string>() { "input/CommonIntl.js" };
		var names = new List<string>() { "CommonStrings", "afrikaans" };

		var parsedMappings = new List<MappingEntry>()
			{
				new(
					generatedSourcePosition: new SourcePosition(0, 0),
					originalSourcePosition: new SourcePosition(1, 0),
					originalName: names[0],
					originalFileName: sources[0]),
				new(
					generatedSourcePosition: new SourcePosition(0, 13),
					originalSourcePosition: new SourcePosition(1, 0),
					null,
					originalFileName: sources[0]),
				new(
					generatedSourcePosition: new SourcePosition(0, 14),
					originalSourcePosition: new SourcePosition(1, 14),
					null,
					originalFileName: sources[0]),
			};

		return new SourceMap(
			version: 3,
			file: "CommonIntl",
			mappings: default,
			sources: sources,
			names: names,
			parsedMappings: parsedMappings,
			sourcesContent: default);
	}
}
