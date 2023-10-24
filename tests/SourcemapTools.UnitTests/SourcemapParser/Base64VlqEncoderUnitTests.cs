using System.Text;
using NUnit.Framework;
using SourcemapTools.SourcemapParser.Internal;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class Base64VlqEncoderUnitTests
{
	[Test]
	public void Base64VlqEncoder_SmallValue_ListWithOnlyOneValue()
	{
		// Act
		var result = new StringBuilder();
		Base64VlqEncoder.Encode(result, 15);

		// Assert
		Assert.That(result.ToString(), Is.EqualTo("e"));
	}

	[Test]
	public void Base64VlqEncoder_LargeValue_ListWithOnlyMultipleValues()
	{
		// Act
		var result = new StringBuilder();
		Base64VlqEncoder.Encode(result, 701);

		// Assert
		Assert.That(result.ToString(), Is.EqualTo("6rB"));
	}

	[Test]
	public void Base64VlqEncoder_NegativeValue_ListWithCorrectValue()
	{
		// Act
		var result = new StringBuilder();
		Base64VlqEncoder.Encode(result, -15);

		// Assert
		Assert.That(result.ToString(), Is.EqualTo("f"));
	}
}
