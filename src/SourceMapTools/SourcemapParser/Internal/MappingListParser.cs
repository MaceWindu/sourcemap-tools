using System;
using System.Collections.Generic;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// One of the entries of the V3 source map is a base64 VLQ encoded string providing metadata about a particular line of generated code.
/// This class is responsible for converting this string into a more friendly format.
/// </summary>
public static class MappingsListParser
{
	private static readonly char[] LineDelimiter = [','];

	/// <summary>
	/// Parses a single "segment" of the mapping field for a source map. A segment describes one piece of code in the generated source.
	/// In the mapping string "AAaAA,CAACC;", AAaAA and CAACC are both segments. This method assumes the segments have already been decoded
	/// from Base64 VLQ into a list of integers.
	/// </summary>
	/// <param name="segmentFields">The integer values for the segment fields.</param>
	/// <param name="mappingsParserState">The current state of the state variables for the parser.</param>
	/// <returns></returns>
	public static NumericMappingEntry ParseSingleMappingSegment(IReadOnlyList<int> segmentFields, MappingsParserState mappingsParserState)
	{
		if (segmentFields == null)
		{
			throw new ArgumentNullException(nameof(segmentFields));
		}

		if (segmentFields.Count is 0 or 2 or 3)
		{
			throw new ArgumentOutOfRangeException(nameof(segmentFields));
		}

		var generatedLineNumber = mappingsParserState.CurrentGeneratedLineNumber;
		var generatedColumnNumber = mappingsParserState.CurrentGeneratedColumnBase + segmentFields[0];

		/*
		 *  The following description was taken from the Sourcemap V3 spec https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/mobilebasic?pref=2&pli=1
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

		int? originalSourceFileIndex = null;
		int? originalLineNumber = null;
		int? originalColumnNumber = null;
		int? originalNameIndex = null;
		if (segmentFields.Count > 1)
		{
			originalSourceFileIndex = mappingsParserState.SourcesListIndexBase + segmentFields[1];
			originalLineNumber = mappingsParserState.OriginalSourceStartingLineBase + segmentFields[2];
			originalColumnNumber = mappingsParserState.OriginalSourceStartingColumnBase + segmentFields[3];
		}

		if (segmentFields.Count >= 5)
		{
			originalNameIndex = mappingsParserState.NamesListIndexBase + segmentFields[4];
		}

		return new NumericMappingEntry(
			generatedLineNumber,
			generatedColumnNumber,
			originalSourceFileIndex,
			originalLineNumber,
			originalColumnNumber,
			originalNameIndex);
	}

	/// <summary>
	/// Top level API that should be called for decoding the MappingsString element. It will convert the string containing Base64
	/// VLQ encoded segments into a list of MappingEntries.
	/// </summary>
	public static IReadOnlyList<MappingEntry> ParseMappings(string mappingString, IReadOnlyList<string> names, IReadOnlyList<string> sources)
	{
		if (mappingString == null)
		{
			throw new ArgumentNullException(nameof(mappingString));
		}

		var mappingEntries = new List<MappingEntry>();
		var currentMappingsParserState = new MappingsParserState();

		// The V3 source map format calls for all Base64 VLQ segments to be separated by commas.
		// Each line of generated code is separated using semicolons. The count of semicolons encountered gives the current line number.
		var lines = mappingString.SplitFast(';');

		for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++)
		{
			// The only value that resets when encountering a semicolon is the starting column.
			currentMappingsParserState = new MappingsParserState(
				currentMappingsParserState,
				newGeneratedLineNumber: lineNumber,
				newGeneratedColumnBase: 0);

			var segmentsForLine = lines[lineNumber].Split(LineDelimiter, StringSplitOptions.RemoveEmptyEntries);

			foreach (var segment in segmentsForLine)
			{
				// Reuse the numericMappingEntry to ease GC allocations.
				var numericMappingEntry = ParseSingleMappingSegment(Base64VlqDecoder.Decode(segment), currentMappingsParserState);
				mappingEntries.Add(numericMappingEntry.ToMappingEntry(names, sources));

				// Update the current MappingParserState based on the generated MappingEntry
				currentMappingsParserState = new MappingsParserState(currentMappingsParserState,
					newGeneratedColumnBase: numericMappingEntry.GeneratedColumnNumber,
					newSourcesListIndexBase: numericMappingEntry.OriginalSourceFileIndex,
					newOriginalSourceStartingLineBase: numericMappingEntry.OriginalLineNumber,
					newOriginalSourceStartingColumnBase: numericMappingEntry.OriginalColumnNumber,
					newNamesListIndexBase: numericMappingEntry.OriginalNameIndex);
			}
		}
		return mappingEntries;
	}
}
