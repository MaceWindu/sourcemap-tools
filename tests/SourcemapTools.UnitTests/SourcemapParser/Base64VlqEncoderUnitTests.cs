using System.Text;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{

	public class Base64VlqEncoderUnitTests
	{
		[Test]
		public void Base64VlqEncoder_SmallValue_ListWithOnlyOneValue()
		{
			// Act
			var result = new StringBuilder();
			Base64VlqEncoder.Encode(result, 15);

			// Assert
			Assert.AreEqual("e", result.ToString());
		}

		[Test]
		public void Base64VlqEncoder_LargeValue_ListWithOnlyMultipleValues()
		{
			// Act
			var result = new StringBuilder();
			Base64VlqEncoder.Encode(result, 701);

			// Assert
			Assert.AreEqual("6rB", result.ToString());
		}

		[Test]
		public void Base64VlqEncoder_NegativeValue_ListWithCorrectValue()
		{
			// Act
			var result = new StringBuilder();
			Base64VlqEncoder.Encode(result, -15);

			// Assert
			Assert.AreEqual("f", result.ToString());
		}

	}
}
