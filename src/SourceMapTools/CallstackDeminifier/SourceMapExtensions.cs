using System;
using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	internal static class SourceMapExtensions
	{
		/// <summary>
		/// Gets the original name corresponding to a function based on the information provided in the source map.
		/// </summary>
		internal static string? GetDeminifiedMethodName(this SourceMap sourceMap, IReadOnlyList<BindingInformation> bindings)
		{
			if (bindings.Count > 0)
			{
				var entryNames = new List<string>();

				foreach (var binding in bindings)
				{
					var entry = sourceMap.GetMappingEntryForGeneratedSourcePosition(binding.SourcePosition);
					if (entry != null && entry.Value.OriginalName != null)
					{
						entryNames.Add(entry.Value.OriginalName);
					}
				}

				// // The object name already contains the method name, so do not append it
				if (entryNames.Count > 1
					&& entryNames[^2].Length > entryNames[^1].Length
					&& entryNames[^2].EndsWith(entryNames[^1], StringComparison.Ordinal)
					&& entryNames[^2][entryNames[^2].Length - 1 - entryNames[^1].Length] == '.')
				{
					entryNames.RemoveAt(entryNames.Count - 1);
				}

				if (entryNames.Count > 2 && entryNames[^2] == "prototype")
				{
					entryNames.RemoveAt(entryNames.Count - 2);
				}

				if (entryNames.Count > 0)
				{
					return string.Join(".", entryNames);
				}
			}

			return null;
		}
	}
}
