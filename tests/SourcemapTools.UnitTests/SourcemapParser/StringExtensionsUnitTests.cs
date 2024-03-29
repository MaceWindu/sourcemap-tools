﻿using NUnit.Framework;
using SourcemapTools.SourcemapParser.Internal;

namespace SourcemapToolkit.SourcemapParser.UnitTests;

public class StringExtensionsUnitTests
{
	[Test]
	public void SplitFast_EmptyInput_Matches()
	{
		// Arrange
		const string input = "";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_OneCharacterNotSplit_Matches()
	{
		// Arrange
		const string input = "A";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_OneCharacterDelimiter_Matches()
	{
		// Arrange
		const string input = ",";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_DelimiterAtStart_Matches()
	{
		// Arrange
		const string input = ",Hello";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_DelimiterAtEnd_Matches()
	{
		// Arrange
		const string input = "Hello,";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_BackToBack_Matches()
	{
		// Arrange
		const string input = "Hello,,World";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_LongSentence_Matches()
	{
		// Arrange
		const string input = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna. Nunc viverra imperdiet enim. Fusce est. Vivamus a tellus. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Proin pharetra nonummy pede. Mauris et orci. Aenean nec lorem. In porttitor. Donec laoreet nonummy augue. Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. Mauris eget neque at sem venenatis eleifend. Ut nonummy.";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}

	[Test]
	public void SplitFast_Complex_Matches()
	{
		// Arrange
		const string input = ",,Hello,World,How,,Are,You,Doing,,";
		const char delimiter = ',';

		// Act
		var fastSplit = StringExtensions.SplitFast(input, delimiter);
		var normalSplit = input.Split(delimiter);

		// Assert
		Assert.That(fastSplit, Has.Length.EqualTo(normalSplit.Length));

		for (var i = 0; i < normalSplit.Length; i++)
		{
			Assert.That(fastSplit[i], Is.EqualTo(normalSplit[i]));
		}
	}
}
