using System;
using NUnit.Framework;
using SourcemapTools.SourcemapParser.Internal;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class Base64ConverterUnitTests
{
	[Test]
	public void FromBase64_ValidBase64InputC_CorrectIntegerOutput2()
	{
		// Act
		var value = Base64Converter.FromBase64('C');

		// Assert
		Assert.That(value, Is.EqualTo(2));
	}

	[Test]
	public void FromBase64_ValidBase64Input9_CorrectIntegerOutput61()
	{
		// Act
		var value = Base64Converter.FromBase64('9');

		// Assert
		Assert.That(value, Is.EqualTo(61));
	}

	[Test]
	public void FromBase64_InvalidBase64Input_ThrowsException() =>
		// Act
		Assert.Throws<ArgumentOutOfRangeException>(() => Base64Converter.FromBase64('@'));

	[Test]
	public void ToBase64_ValidIntegerInput61_CorrectBase64Output9()
	{
		// Act
		var value = Base64Converter.ToBase64(61);

		// Assert
		Assert.That(value, Is.EqualTo('9'));
	}

	[Test]
	public void ToBase64_NegativeIntegerInput_ThrowsException() =>
		// Act
		Assert.Throws<ArgumentOutOfRangeException>(() => Base64Converter.ToBase64(-1));

	[Test]
	public void ToBase64_InvalidIntegerInput_ThrowsException() =>
		// Act
		Assert.Throws<ArgumentOutOfRangeException>(() => Base64Converter.ToBase64(64));
}
