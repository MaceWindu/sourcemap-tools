using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{

	public class SourcePositionUnitTests
	{
		[Test]
		public void CompareTo_SameLineAndColumn_Equal()
		{
			// Arrange
			var x = new SourcePosition(1, 6);
			var y = new SourcePosition(1, 6);

			// Act
			var result = x.CompareTo(y);

			// Assert
			Assert.AreEqual(0, result);
		}

		[Test]
		public void CompareTo_LargerZeroBasedLineNumber_ReturnLarger()
		{
			// Arrange
			var x = new SourcePosition(2, 4);
			var y = new SourcePosition(1, 8);

			// Act
			var result = x.CompareTo(y);

			// Assert
			Assert.True(result > 0);
		}

		[Test]
		public void CompareTo_SmallerZeroBasedLineNumber_ReturnSmaller()
		{
			// Arrange
			var x = new SourcePosition(1, 4);
			var y = new SourcePosition(3, 8);

			// Act
			var result = x.CompareTo(y);

			// Assert
			Assert.True(result < 0);
		}

		[Test]
		public void CompareTo_SameLineLargerColumn_ReturnLarger()
		{
			// Arrange
			var x = new SourcePosition(1, 8);
			var y = new SourcePosition(1, 6);

			// Act
			var result = x.CompareTo(y);

			// Assert
			Assert.True(result > 0);
		}

		[Test]
		public void CompareTo_SameLineSmallerColumn_ReturnSmaller()
		{
			// Arrange
			var x = new SourcePosition(1, 4);
			var y = new SourcePosition(1, 8);

			// Act
			var result = x.CompareTo(y);

			// Assert
			Assert.True(result < 0);
		}

		[Test]
		public void LessThanOverload_SameZeroBasedLineNumberXColumnSmaller_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(1, 4);
			var y = new SourcePosition(1, 8);

			// Act
			var result = x < y;

			// Assert
			Assert.True(result);
		}

		[Test]
		public void LessThanOverload_XZeroBasedLineNumberSmallerX_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(1, 10);
			var y = new SourcePosition(2, 8);

			// Act
			var result = x < y;

			// Assert
			Assert.True(result);
		}

		[Test]
		public void LessThanOverload_Equal_ReturnFalse()
		{
			// Arrange
			var x = new SourcePosition(0, 0);
			var y = new SourcePosition(0, 0);

			// Act
			var result = x < y;

			// Assert
			Assert.False(result);
		}

		[Test]
		public void LessThanOverload_XColumnLarger_ReturnFalse()
		{
			// Arrange
			var x = new SourcePosition(0, 10);
			var y = new SourcePosition(0, 0);

			// Act
			var result = x < y;

			// Assert
			Assert.False(result);
		}

		[Test]
		public void GreaterThanOverload_SameLineXColumnLarger_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(2, 10);
			var y = new SourcePosition(2, 3);

			// Act
			var result = x > y;

			// Assert
			Assert.True(result);
		}

		[Test]
		public void GreaterThanOverload_XZeroBasedLineNumberLarger_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(3, 10);
			var y = new SourcePosition(2, 30);

			// Act
			var result = x > y;

			// Assert
			Assert.True(result);
		}

		[Test]
		public void GreaterThanOverload_Equal_ReturnFalse()
		{
			// Arrange
			var x = new SourcePosition(3, 10);
			var y = new SourcePosition(3, 10);

			// Act
			var result = x > y;

			// Assert
			Assert.False(result);
		}

		[Test]
		public void GreaterThanOverload_XSmaller_ReturnFalse()
		{
			// Arrange
			var x = new SourcePosition(3, 9);
			var y = new SourcePosition(3, 10);

			// Act
			var result = x > y;

			// Assert
			Assert.False(result);
		}

		[Test]
		public void IsEqualish_XEqualsY_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(13, 5);
			var y = new SourcePosition(13, 5);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.True(result);
		}

		[Test]
		public void IsEqualish_XColumnNumberBiggerByOne_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(2, 8);
			var y = new SourcePosition(2, 7);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.True(result);
		}

		[Test]
		public void IsEqualish_YColumnNumberBiggerByOne_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(1, 10);
			var y = new SourcePosition(1, 11);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.True(result);
		}

		[Test]
		public void IsEqualish_YColumnNumberBiggerByTwo_ReturnFalse()
		{
			// Arrange
			var x = new SourcePosition(155, 100);
			var y = new SourcePosition(155, 102);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.False(result);
		}

		[Test]
		public void IsEqualish_XColumnNumberZeroLineNumbersDifferByOne_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(235, 0);
			var y = new SourcePosition(234, 102);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.True(result);
		}

		[Test]
		public void IsEqualish_YColumnNumberZeroLineNumbersDifferByOne_ReturnTrue()
		{
			// Arrange
			var x = new SourcePosition(458, 13);
			var y = new SourcePosition(459, 0);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.True(result);
		}

		[Test]
		public void IsEqualish_YColumnNumberZeroLineNumbersDifferByTwo_ReturnFalse()
		{
			// Arrange
			var x = new SourcePosition(5456, 13);
			var y = new SourcePosition(5458, 0);

			// Act
			var result = x.IsEqualish(y);

			// Assert
			Assert.False(result);
		}
	}
}