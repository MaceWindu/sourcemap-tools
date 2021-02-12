﻿using System;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Identifies the location of a piece of code in a JavaScript file.
	/// </summary>
	public class SourcePosition : IComparable<SourcePosition>
	{
		/// <summary>
		/// Gets zero-based position line number.
		/// </summary>
		public int Line { get; internal set; }

		/// <summary>
		/// Gets zero-based position column number.
		/// </summary>
		public int Column { get; internal set; }

		internal SourcePosition()
			: this(0, 0)
		{
		}

		public SourcePosition(int line, int column)
		{
			Line = line;
			Column = column;
		}

		public int CompareTo(SourcePosition other)
		{
			if (Line == other.Line)
			{
				return Column.CompareTo(other.Column);
			}

			return Line.CompareTo(other.Line);
		}

		public static bool operator <(SourcePosition x, SourcePosition y) => x.CompareTo(y) < 0;

		public static bool operator >(SourcePosition x, SourcePosition y) => x.CompareTo(y) > 0;

		/// <summary>
		/// Returns true if we think that the two source positions are close enough together that they may in fact be the referring to the same piece of code.
		/// </summary>
		public bool IsEqualish(SourcePosition other)
		{
			// If the column numbers differ by 1, we can say that the source code is approximately equal
			if (Line == other.Line && Math.Abs(Column - other.Column) <= 1)
			{
				return true;
			}

			// This handles the case where we are comparing code at the end of one line and the beginning of the next line.
			// If the column number on one of the entries is zero, it is ok for the line numbers to differ by 1, so long as the one that has a column number of zero is the one with the larger line number.
			// Since we don't have the number of columns in each line, we can't know for sure if these two pieces of code are actually near each other. This is an optimistic guess.
			if (Math.Abs(Line - other.Line) == 1)
			{
				var largerLineNumber = Line > other.Line ? this : other;

				if (largerLineNumber.Column == 0)
				{
					return true;
				}
			}

			return false;
		}

		public SourcePosition Clone() => new SourcePosition(Line, Column);
	}
}
