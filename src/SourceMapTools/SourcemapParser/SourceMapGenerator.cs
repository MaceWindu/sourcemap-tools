using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Provides utility methods for source map serialization.
	/// </summary>
	public static class SourceMapGenerator
	{
		/// <summary>
		/// Convenience wrapper around SerializeMapping, but returns a base 64 encoded string instead
		/// </summary>
		public static string GenerateSourceMapInlineComment(SourceMap sourceMap, JsonSerializerOptions? jsonSerializerSettings = null)
		{
			var mappings = SerializeMapping(sourceMap, jsonSerializerSettings);
			var bytes = Encoding.UTF8.GetBytes(mappings);
			var encoded = Convert.ToBase64String(bytes);

			return @"//# sourceMappingURL=data:application/json;base64," + encoded;
		}

		/// <summary>
		/// Serialize SourceMap object to json string with given serialize settings
		/// </summary>
		public static string SerializeMapping(SourceMap sourceMap, JsonSerializerOptions? jsonSerializerSettings = null)
		{
			if (sourceMap == null)
			{
				throw new ArgumentNullException(nameof(sourceMap));
			}

			string? mappings = null;
			if (sourceMap.ParsedMappings.Count > 0)
			{
				var state = new MappingGenerateState(sourceMap.Names ?? new List<string>(), sourceMap.Sources ?? new List<string>());
				var output = new StringBuilder();

				foreach (var entry in sourceMap.ParsedMappings)
				{
					SerializeMappingEntry(entry, state, output);
				}

				output.Append(';');

				mappings = output.ToString();
			}

			var mapToSerialize = new SourceMap(
				sourceMap.Version,
				sourceMap.File,
				mappings,
				sourceMap.Sources,
				sourceMap.Names,
				null!,
				sourceMap.SourcesContent);

			return JsonSerializer.Serialize(mapToSerialize,
				jsonSerializerSettings ?? new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
				});
		}

		/// <summary>
		/// Convert each mapping entry to VLQ encoded segments
		/// </summary>
		internal static void SerializeMappingEntry(MappingEntry entry, MappingGenerateState state, StringBuilder output)
		{
			if (state.LastGeneratedPosition.Line > entry.GeneratedSourcePosition.Line)
			{
				throw new InvalidOperationException($"Invalid sourmap detected. Please check the line {entry.GeneratedSourcePosition.Line}");
			}

			// Each line of generated code is separated using semicolons
			while (entry.GeneratedSourcePosition.Line != state.LastGeneratedPosition.Line)
			{
				state.AdvanceLastGeneratedPositionLine();
				state.IsFirstSegment = true;
				output.Append(';');
			}

			// The V3 source map format calls for all Base64 VLQ segments to be seperated by commas.
			if (!state.IsFirstSegment)
			{
				output.Append(',');
			}

			state.IsFirstSegment = false;

			/*
			 *	The following description was taken from the Sourcemap V3 spec https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/mobilebasic?pref=2&pli=1
			 *  The Sourcemap V3 spec is under a Creative Commons Attribution-ShareAlike 3.0 Unported License. https://creativecommons.org/licenses/by-sa/3.0/
			 *  
			 *  Each VLQ segment has 1, 4, or 5 variable length fields.
			 *  The fields in each segment are:
			 *  1. The zero-based starting column of the line in the generated code that the segment represents. 
			 *     If this is the first field of the first segment, or the first segment following a new generated line(“;”), 
			 *     then this field holds the whole base 64 VLQ.Otherwise, this field contains a base 64 VLQ that is relative to
			 *     the previous occurrence of this field.Note that this is different than the fields below because the previous 
			 *     value is reset after every generated line.
			 *  2. If present, an zero - based index into the “sources” list.This field is a base 64 VLQ relative to the previous
			 *     occurrence of this field, unless this is the first occurrence of this field, in which case the whole value is represented.
			 *  3. If present, the zero-based starting line in the original source represented. This field is a base 64 VLQ relative to the
			 *     previous occurrence of this field, unless this is the first occurrence of this field, in which case the whole value is 
			 *     represented.Always present if there is a source field.
			 *  4. If present, the zero - based starting column of the line in the source represented.This field is a base 64 VLQ relative to 
			 *     the previous occurrence of this field, unless this is the first occurrence of this field, in which case the whole value is
			 *     represented.Always present if there is a source field.
			 *  5. If present, the zero - based index into the “names” list associated with this segment.This field is a base 64 VLQ relative 
			 *     to the previous occurrence of this field, unless this is the first occurrence of this field, in which case the whole value
			 *     is represented.
			 */

			Base64VlqEncoder.Encode(output, entry.GeneratedSourcePosition.Column - state.LastGeneratedPosition.Column);
			state.UpdateLastGeneratedPositionColumn(entry.GeneratedSourcePosition.Column);

			if (entry.OriginalFileName != null)
			{
				var sourceIndex = state.Sources.IndexOf(entry.OriginalFileName);
				if (sourceIndex < 0)
				{
					throw new SerializationException("Source map contains original source that cannot be found in provided sources array");
				}

				Base64VlqEncoder.Encode(output, sourceIndex - state.LastSourceIndex);
				state.LastSourceIndex = sourceIndex;

				Base64VlqEncoder.Encode(output, entry.OriginalSourcePosition!.Line - state.LastOriginalPosition.Line);

				Base64VlqEncoder.Encode(output, entry.OriginalSourcePosition.Column - state.LastOriginalPosition.Column);

				state.LastOriginalPosition = entry.OriginalSourcePosition;

				if (entry.OriginalName != null)
				{
					var nameIndex = state.Names.IndexOf(entry.OriginalName);
					if (nameIndex < 0)
					{
						throw new SerializationException("Source map contains original name that cannot be found in provided names array");
					}

					Base64VlqEncoder.Encode(output, nameIndex - state.LastNameIndex);
					state.LastNameIndex = nameIndex;
				}
			}
		}
	}
}
