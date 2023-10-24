using NUnit.Framework;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class KeyValueCacheUnitTests
{
	[Test]
	public void GetValue_KeyNotInCache_CallValueGetter()
	{
		// Arrange
		static string? valueGetter(string x)
		{
			return x == "bar" ? "foo" : null;
		}

		var keyValueCache = new KeyValueCache<string, string>(valueGetter);

		// Act
		var result = keyValueCache.GetValue("bar");

		// Assert
		Assert.That(result, Is.EqualTo("foo"));
	}

	[Test]
	public void GetValue_CallGetTwice_OnlyCallValueGetterOnce()
	{
		// Arrange
		var cnt = 0;
		string? valueGetter(string x)
		{
			if (x == "bar")
			{
				cnt++;
				return "foo";
			}

			return null;
		}

		var keyValueCache = new KeyValueCache<string, string>(valueGetter);
		keyValueCache.GetValue("bar"); // Place the value in the cache

		// Act
		var result = keyValueCache.GetValue("bar");

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result, Is.EqualTo("foo"));
			Assert.That(cnt, Is.EqualTo(1));
		});
	}

	[Test]
	public void GetValue_CallGetTwiceValueGetterReturnsNull_CallGetterTwice()
	{
		// Arrange
		var cnt = 0;
		string? valueGetter(string x)
		{
			if (x == "bar")
			{
				cnt++;
			}

			return null;
		}
		var keyValueCache = new KeyValueCache<string, string>(valueGetter);
		keyValueCache.GetValue("bar"); // Place null in the cache

		// Act
		var result = keyValueCache.GetValue("bar");

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result, Is.Null);
			Assert.That(cnt, Is.EqualTo(2));
		});
	}

	[Test]
	public void GetValue_CallGetMultipleTimesFirstGetterReturnsNull_CacheFirstNonNullValue()
	{
		// Arrange
		var cnt = 0;
		string? returnValue = null;
		string? valueGetter(string x)
		{
			if (x == "bar")
			{
				cnt++;
				return returnValue;
			}

			return null;
		}
		var keyValueCache = new KeyValueCache<string, string>(valueGetter);
		keyValueCache.GetValue("bar"); // Place null in the cache
		Assert.That(cnt, Is.EqualTo(1));

		returnValue = "foo";
		keyValueCache.GetValue("bar"); // Place a non null value in the cache

		// Act
		var result = keyValueCache.GetValue("bar");

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(result, Is.EqualTo("foo"));
			Assert.That(cnt, Is.EqualTo(2));
		});
	}
}
