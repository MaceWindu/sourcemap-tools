using System.Collections.Generic;
using NUnit.Framework;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
	public class IReadOnlyListExtensionsUnitTests
	{
		[Test]
		public void IndexOf_ValueInList_CorrectlyReturnsIndex()
		{
			// Arrange
			var list = new[] { 6, 4, 1, 8 };

			// Act
			var index = IReadOnlyListExtensions.IndexOf(list, 1);

			// Assert
			Assert.AreEqual(2, index);
		}

		[Test]
		public void IndexOf_ValueNotInList_CorrectlyReturnsNegativeOne()
		{
			// Arrange
			var list = new[] { 2, 4, 6, 8 };

			// Act
			var index = IReadOnlyListExtensions.IndexOf(list, 1);

			// Assert
			Assert.AreEqual(-1, index);
		}

		[Test]
		public void IndexOf_ValueAppearsMultipleTimes_CorrectlyReturnsFirstInstance()
		{
			// Arrange
			IReadOnlyList<int> list = new[] { 2, 4, 6, 8, 4 };

			// Act
			var index = IReadOnlyListExtensions.IndexOf(list, 4);

			// Assert
			Assert.AreEqual(1, index);
		}

		[Test]
		public void BinarySearch_EvenNumberOfElements_CorrectlyMatchesListImplementation()
		{
			// Arrange
			// 6 elements total
			const int minFillIndexInclusive = 1;
			const int maxFillIndexInclusive = 4;

			var comparer = Comparer<int>.Default;
			var list = new List<int>();

			for (var i = minFillIndexInclusive; i <= maxFillIndexInclusive; i++)
			{
				list.Add(2 * i); // multiplying each entry by 2 to make sure there are gaps
			}

			// Act & Assert
			for (var i = minFillIndexInclusive - 1; i <= maxFillIndexInclusive + 1; i++)
			{
				Assert.AreEqual(list.BinarySearch(i, comparer), IReadOnlyListExtensions.BinarySearch(list, i, comparer));
				Assert.AreEqual(list.BinarySearch(i + 1, comparer), IReadOnlyListExtensions.BinarySearch(list, i + 1, comparer));
			}
		}

		[Test]
		public void BinarySearch_OddNumberOfElements_CorrectlyMatchesListImplementation()
		{
			// Arrange
			// 6 elements total
			const int minFillIndexInclusive = 1;
			const int maxFillIndexInclusive = 5;

			var comparer = Comparer<int>.Default;
			var list = new List<int>();

			for (var i = minFillIndexInclusive; i <= maxFillIndexInclusive; i++)
			{
				list.Add(2 * i); // multiplying each entry by 2 to make sure there are gaps
			}

			// Act & Assert
			for (var i = minFillIndexInclusive - 1; i <= maxFillIndexInclusive + 1; i++)
			{
				Assert.AreEqual(list.BinarySearch(i, comparer), IReadOnlyListExtensions.BinarySearch(list, i, comparer));
				Assert.AreEqual(list.BinarySearch(i + 1, comparer), IReadOnlyListExtensions.BinarySearch(list, i + 1, comparer));
			}
		}
	}
}
