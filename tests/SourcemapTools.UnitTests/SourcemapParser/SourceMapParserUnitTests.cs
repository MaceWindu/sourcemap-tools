using System.IO;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class SourceMapParserUnitTests
{
	[Test]
	public void ParseSourceMap_NullInputStream_ReturnsNull()
	{
		// Arrange
		Stream? input = null;

		// Act
		var output = SourceMapParser.ParseSourceMap(input);

		// Assert
		Assert.That(output, Is.Null);
	}

	[Test]
	public void ParseSourceMap_SimpleSourceMap_CorrectlyParsed()
	{
		// Arrange
		var input = /*lang=json,strict*/ "{ \"version\":3, \"file\":\"CommonIntl\", \"lineCount\":65, \"mappings\":\"AACAA,aAAA,CAAc\", \"sources\":[\"input/CommonIntl.js\"], \"names\":[\"CommonStrings\",\"afrikaans\"]}";
		using var stream = UnitTestUtils.StreamFromString(input);

		// Act
		var output = SourceMapParser.ParseSourceMap(stream);

		// Assert
		Assert.That(output, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(output.Version, Is.EqualTo(3));
			Assert.That(output.File, Is.EqualTo("CommonIntl"));
			Assert.That(output.Mappings, Is.EqualTo("AACAA,aAAA,CAAc"));
			Assert.That(output.Sources, Is.Not.Null);
		});
		Assert.That(output.Sources, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(output.Sources[0], Is.EqualTo("input/CommonIntl.js"));
			Assert.That(output.Names, Is.Not.Null);
		});
		Assert.That(output.Names, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(output.Names[0], Is.EqualTo("CommonStrings"));
			Assert.That(output.Names[1], Is.EqualTo("afrikaans"));
		});
	}
}
