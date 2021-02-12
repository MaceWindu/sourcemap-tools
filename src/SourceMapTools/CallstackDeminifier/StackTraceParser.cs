﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Class used to parse a JavaScript stack trace 
	/// string into a list of StackFrame objects
	/// </summary>
	public class StackTraceParser : IStackTraceParser
	{
		private static readonly Regex _lineNumberRegex = new Regex(@"([^@(\s]*\.js)[^/]*:([0-9]+):([0-9]+)[^/]*$", RegexOptions.Compiled);

		/// <summary>
		/// Generates a list of StackFrame objects based on the input stack trace.
		/// This method normalizes differences between different browsers.
		/// The source positions in the parsed stack frames will be normalized so they
		/// are zero-based instead of one-based to align with the rest of the library.
		/// </summary>
		/// <returns>
		/// Returns a list of StackFrame objects corresponding to the stackTraceString.
		/// Any parts of the stack trace that could not be parsed are excluded from
		/// the result. Does not ever return null.
		/// </returns>
		/// <remarks>
		/// This override drops the Message out param for backward compatibility
		/// </remarks>
		IReadOnlyList<StackFrame> IStackTraceParser.ParseStackTrace(string stackTraceString)
		{
			return ParseStackTrace(stackTraceString, out var _);
		}

		/// <summary>
		/// Generates a list of StackFrame objects based on the input stack trace.
		/// This method normalizes differences between different browsers.
		/// The source positions in the parsed stack frames will be normalized so they
		/// are zero-based instead of one-based to align with the rest of the library.
		/// </summary>
		/// <returns>
		/// Returns a list of StackFrame objects corresponding to the stackTraceString.
		/// Any parts of the stack trace that could not be parsed are excluded from
		/// the result. Does not ever return null.
		/// </returns>
		public virtual IReadOnlyList<StackFrame> ParseStackTrace(string stackTraceString, out string? message)
		{
			if (stackTraceString == null)
			{
				throw new ArgumentNullException(nameof(stackTraceString));
			}

			message = null;
			var stackTrace = new List<StackFrame>();
			var stackFrameStrings = stackTraceString.Split('\n').ToList();

			var firstFrame = stackFrameStrings.First();
			if (!firstFrame.StartsWith(" ", StringComparison.Ordinal) && TryExtractMethodNameFromFrame(firstFrame) == null)
			{
				message = firstFrame.Trim();
				stackFrameStrings.RemoveAt(0);
			}

			foreach (var frame in stackFrameStrings)
			{
				var parsedStackFrame = TryParseSingleStackFrame(frame);

				if (parsedStackFrame != null)
				{
					stackTrace.Add(parsedStackFrame);
				}
			}

			return stackTrace;
		}

		/// <summary>
		/// Given a single stack frame, extract the method name.
		/// </summary>
		protected virtual string? TryExtractMethodNameFromFrame(string frame)
		{
			string? methodName = null;

			// Firefox has stackframes in the form: "c@http://localhost:19220/crashcauser.min.js:1:34"
			var atSymbolIndex = frame.IndexOf("@http", StringComparison.Ordinal);
			if (atSymbolIndex != -1)
			{
				methodName = frame.Substring(0, atSymbolIndex).TrimStart();
			}
			else
			{
				// Chrome and IE11 have stackframes in the form: " at d (http://chrisgocallstack.azurewebsites.net/crashcauser.min.js:1:75)"
				var atStringIndex = frame.IndexOf("at ", StringComparison.Ordinal);
				if (atStringIndex != -1)
				{
					var httpIndex = frame.IndexOf(" (http", atStringIndex, StringComparison.Ordinal);
					if (httpIndex == -1)
					{
						httpIndex = frame.IndexOf(" http", atStringIndex, StringComparison.Ordinal);
						if (httpIndex != -1)
						{
							httpIndex++;        // append one char to include a blank space to be able to replace "at " correctly
						}
					}

					if (httpIndex != -1)
					{
						methodName = frame.Substring(atStringIndex, httpIndex - atStringIndex).Replace("at ", "").Trim();
					}
					else
					{
						var parenthesesIndex = frame.IndexOf(" (", atStringIndex, StringComparison.Ordinal);
						if (parenthesesIndex != -1)
						{
							methodName = frame.Substring(atStringIndex, parenthesesIndex - atStringIndex).Replace("at ", "").Trim();
						}
					}
				}
			}

			if (string.IsNullOrWhiteSpace(methodName))
			{
				methodName = null;
			}

			return methodName;
		}

		/// <summary>
		/// Parses a string representing a single stack frame into a StackFrame object. 
		/// </summary>
		protected internal virtual StackFrame? TryParseSingleStackFrame(string frame)
		{
			if (frame == null)
			{
				throw new ArgumentNullException(nameof(frame));
			}

			var lineNumberMatch = _lineNumberRegex.Match(frame);

			if (!lineNumberMatch.Success)
			{
				return null;
			}

			var result = new StackFrame(TryExtractMethodNameFromFrame(frame));

			if (lineNumberMatch.Success)
			{
				result.FilePath = lineNumberMatch.Groups[1].Value;
				result.SourcePosition = new SourcePosition(
					// The browser provides one-based line and column numbers, but the
					// rest of this library uses zero-based values. Normalize to make
					// the stack frames zero based.
					int.Parse(lineNumberMatch.Groups[2].Value, CultureInfo.InvariantCulture) - 1,
					int.Parse(lineNumberMatch.Groups[3].Value, CultureInfo.InvariantCulture) - 1);
			}

			return result;
		}
	}
}