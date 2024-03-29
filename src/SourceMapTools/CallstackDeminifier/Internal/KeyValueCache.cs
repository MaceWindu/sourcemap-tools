﻿using System;
using System.Collections.Concurrent;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Internal API.
/// </summary>
public sealed class KeyValueCache<TKey, TValue>(Func<TKey, TValue?> valueGetter) where TValue : class
{
	private readonly ConcurrentDictionary<TKey, TValue?> _cache = new();
	private readonly Func<TKey, TValue?> _valueGetter = valueGetter;

	/// <summary>
	/// Attempts to obtain the value associated with this key from the cache.
	/// If it is not found in the cache, it gets it from the valueGetter function provided
	/// and stores it in the cache for future calls.
	/// </summary>
	public TValue? GetValue(TKey key)
	{
		if (!_cache.TryGetValue(key, out var value))
		{
			value = _cache.GetOrAdd(key, _valueGetter);
		}
		else if (value == null)
		{
			// If the value stored in the cache is null, we should see if we can now get a
			// non-null value from the value getter
			_cache.TryUpdate(key, _valueGetter(key), null);
			_cache.TryGetValue(key, out value);
		}

		return value;
	}
}
