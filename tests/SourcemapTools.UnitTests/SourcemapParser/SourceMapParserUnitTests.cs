using System.IO;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{

	public class SourceMapParserUnitTests
	{
		[Test]
		public void ParseSourceMap_NullInputStream_ReturnsNull()
		{
			// Arrange
			Stream? input = null;

			// Act
			var output = SourceMapParser.ParseSourceMap(input!);

			// Assert
			Assert.Null(output);
		}

		[Test]
		public void ParseSourceMap_SimpleSourceMap_CorrectlyParsed()
		{
			// Arrange
			var input = "{ \"version\":3, \"file\":\"CommonIntl\", \"lineCount\":65, \"mappings\":\"AACAA,aAAA,CAAc\", \"sources\":[\"input/CommonIntl.js\"], \"names\":[\"CommonStrings\",\"afrikaans\"]}";

			// Act
			var output = SourceMapParser.ParseSourceMap(UnitTestUtils.StreamFromString(input))!;

			// Assert
			Assert.AreEqual(3, output.Version);
			Assert.AreEqual("CommonIntl", output.File);
			Assert.AreEqual("AACAA,aAAA,CAAc", output.Mappings);
			Assert.AreEqual(1, output.Sources?.Count);
			Assert.AreEqual("input/CommonIntl.js", output.Sources![0]);
			Assert.AreEqual(2, output.Names!.Count);
			Assert.AreEqual("CommonStrings", output.Names[0]);
			Assert.AreEqual("afrikaans", output.Names[1]);
		}
	}
}
