using System;

namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// Internal API.
/// </summary>
public static class StringExtensions
{
	/// <summary>
	/// String.Split() allocates an O(input.Length) int array, and
	/// is surprisingly expensive.  For most cases this implementation
	/// is faster and does fewer allocations.
	/// </summary>
	public static string[] SplitFast(this string input, char delimiter)
	{
		if (input == null)
		{
			throw new ArgumentNullException(nameof(input));
		}

		var segmentCount = 1;
		for (var i = 0; i < input.Length; i++)
		{
			if (input[i] == delimiter)
			{
				segmentCount++;
			}
		}

		var segmentId = 0;
		var prevDelimiter = 0;
		var segments = new string[segmentCount];

		for (var i = 0; i < input.Length; i++)
		{
			if (input[i] == delimiter)
			{
				segments[segmentId] = input[prevDelimiter..i];
				segmentId++;
				prevDelimiter = i + 1;
			}
		}

		segments[segmentId] = input[prevDelimiter..];

		return segments;
	}
}
