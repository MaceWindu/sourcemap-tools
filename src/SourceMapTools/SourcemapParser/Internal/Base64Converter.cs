using System;
using System.Collections.Generic;
using System.Linq;

namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// Used to convert Base64 characters values into integers.
/// </summary>
public static class Base64Converter
{
	private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
	private static readonly Dictionary<char, int> _base64DecodeMap = Base64Alphabet.Select((c, i) => (c, i)).ToDictionary(x => x.c, x => x.i);

	/// <summary>
	/// Converts a base64 value to an integer.
	/// </summary>
	public static int FromBase64(char base64Value)
		=> !_base64DecodeMap.TryGetValue(base64Value, out var result)
			? throw new ArgumentOutOfRangeException(nameof(base64Value), "Tried to convert an invalid base64 value")
			: result;

	/// <summary>
	/// Converts a integer to base64 value.
	/// </summary>
	public static char ToBase64(int value)
		=> value < 0 || value >= Base64Alphabet.Length
		? throw new ArgumentOutOfRangeException(nameof(value))
		: Base64Alphabet[value];
}
