using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{

	public class SourceMapGeneratorUnitTests
	{
		[Fact]
		public void SerializeMappingEntry_DifferentLineNumber_SemicolonAdded()
		{
			// Arrange
			var state = new MappingGenerateState(new List<string>() { "Name" }, new List<string>() { "Source" });
			state.LastGeneratedPosition.Column = 1;

			var entry = new MappingEntry(
				new SourcePosition(1, 0),
				new SourcePosition(1, 0),
				state.Names[0],
				state.Sources[0]);

			// Act
			var result = new StringBuilder();
			SourceMapGenerator.SerializeMappingEntry(entry, state, result);

			// Assert
			Assert.True(result.ToString().IndexOf(';') >= 0);
		}

		[Fact]
		public void SerializeMappingEntry_NoOriginalFileName_OneSegment()
		{
			// Arrange
			var state = new MappingGenerateState(new List<string>() { "Name" }, new List<string>() { "Source" });

			var entry = new MappingEntry(
				new SourcePosition(0, 10),
				new SourcePosition(0, 1),
				null, null);

			// Act
			var result = new StringBuilder();
			SourceMapGenerator.SerializeMappingEntry(entry, state, result);

			// Assert
			Assert.Equal("U", result.ToString());
		}

		[Fact]
		public void SerializeMappingEntry_WithOriginalFileNameNoOriginalName_FourSegments()
		{
			// Arrange
			var state = new MappingGenerateState(new List<string>() { "Name" }, new List<string>() { "Source" })
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
			Assert.Equal(",UAAK", result.ToString());
		}

		[Fact]
		public void SerializeMappingEntry_WithOriginalFileNameAndOriginalName_FiveSegments()
		{
			// Arrange
			var state = new MappingGenerateState(new List<string>() { "Name" }, new List<string>() { "Source" });
			state.LastGeneratedPosition.Line = 1;

			var entry = new MappingEntry(
				new SourcePosition(1, 5),
				new SourcePosition(1, 6),
				state.Names[0],
				state.Sources[0]);

			// Act
			var result = new StringBuilder();
			SourceMapGenerator.SerializeMappingEntry(entry, state, result);

			// Assert
			Assert.Equal("KACMA", result.ToString());
		}

		[Fact]
		public void SerializeMapping_NullInput_ThrowsException()
		{
			// Arrange
			SourceMap? input = null;

			// Act
			Assert.Throws<ArgumentNullException>(() => SourceMapGenerator.SerializeMapping(input!));
		}

		[Fact]
		public void SerializeMapping_SimpleSourceMap_CorrectlySerialized()
		{
			// Arrange
			var input = GetSimpleSourceMap();

			// Act
			var output = SourceMapGenerator.SerializeMapping(input);

			// Assert
			Assert.Equal("{\"version\":3,\"file\":\"CommonIntl\",\"mappings\":\"AACAA,aAAA,CAAc;\",\"sources\":[\"input/CommonIntl.js\"],\"names\":[\"CommonStrings\",\"afrikaans\"]}", output);
		}

		[Fact]
		public void SerializeMappingIntoBast64_NullInput_ThrowsException()
		{
			// Arrange
			SourceMap? input = null;

			// Act
			Assert.Throws<ArgumentNullException>(() => SourceMapGenerator.GenerateSourceMapInlineComment(input!));
		}

		[Fact]
		public void SerializeMappingBase64_SimpleSourceMap_CorrectlySerialized()
		{
			// Arrange
			var input = GetSimpleSourceMap();

			// Act
			var output = SourceMapGenerator.GenerateSourceMapInlineComment(input);

			// Assert
			Assert.Equal("//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiQ29tbW9uSW50bCIsIm1hcHBpbmdzIjoiQUFDQUEsYUFBQSxDQUFjOyIsInNvdXJjZXMiOlsiaW5wdXQvQ29tbW9uSW50bC5qcyJdLCJuYW1lcyI6WyJDb21tb25TdHJpbmdzIiwiYWZyaWthYW5zIl19", output);
		}

		private static SourceMap GetSimpleSourceMap()
		{
			var input = new SourceMap()
			{
				File = "CommonIntl",
				Names = new List<string>() { "CommonStrings", "afrikaans" },
				Sources = new List<string>() { "input/CommonIntl.js" },
				Version = 3,
			};
			input.ParsedMappings.AddRange(new MappingEntry[]
			{
				new MappingEntry(
					new SourcePosition(0, 0),
					new SourcePosition(1, 0),
					input.Names[0],
					input.Sources[0]),
				new MappingEntry(
					new SourcePosition(0, 13),
					new SourcePosition(1, 0),
					null,
					input.Sources[0]),
				new MappingEntry(
					new SourcePosition(0, 14),
					new SourcePosition(1, 14),
					null,
					input.Sources[0])
			});

			return input;
		}
	}
}
