using System.IO;
using System.Text;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
	public static class UnitTestUtils
	{
		public static Stream StreamFromString(string streamContents)
		{
			var byteArray = Encoding.UTF8.GetBytes(streamContents);
			return new MemoryStream(byteArray);
		}

		public static MappingEntry GetSimpleEntry(SourcePosition generatedSourcePosition, SourcePosition originalSourcePosition, string originalFileName) => new(
			generatedSourcePosition,
			originalSourcePosition,
			null,
			originalFileName);

		public static SourcePosition GenerateSourcePosition(int lineNumber, int colNumber = 0) => new(lineNumber, colNumber);
	}
}