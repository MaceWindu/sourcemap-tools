using System;

namespace SourcemapToolkit.SourcemapParser
{
	/// <summary>
	/// Identifies the location of a piece of code in a JavaScript file
	/// </summary>
	public struct SourcePosition : IComparable<SourcePosition>, IEquatable<SourcePosition>
	{
		/// <summary>
		/// Unresolved stack frame postion token.
		/// </summary>
		public static readonly SourcePosition NotFound = new SourcePosition(-1, -1);

		/// <summary>
		/// Zero-based position line number.
		/// </summary>
		public readonly int Line;
		/// <summary>
		/// Zero-based position column number.
		/// </summary>
		public readonly int Column;

		/// <summary>
		/// Creates position instance with specified coordinates.
		/// </summary>
		/// <param name="line">Zero-based position line number.</param>
		/// <param name="column">Zero-based position column number.</param>
		public SourcePosition(int line, int column)
		{
			Line = line;
			Column = column;
		}

		/// <summary>
		/// Compares current position instance with another position.
		/// </summary>
		/// <param name="other">Position to compare to.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item>-1: current position located before other position</item>
		/// <item>0: both positions are the same</item>
		/// <item>1: current position located after other position</item>
		/// </list>
		/// </returns>
		public int CompareTo(SourcePosition other)
		{
			if (Line == other.Line)
			{
				return Column.CompareTo(other.Column);
			}

			return Line.CompareTo(other.Line);
		}

		/// <summary>
		/// Checks if left position is going before right position.
		/// </summary>
		/// <param name="x">left position.</param>
		/// <param name="y">right position.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: left postion goes before right position.</item>
		/// <item><c>false</c>: left postion is same or goes after right position.</item>
		/// </list>
		/// </returns>
		public static bool operator <(SourcePosition x, SourcePosition y)
		{
			return x.CompareTo(y) < 0;
		}

		/// <summary>
		/// Checks if left position is going after right position.
		/// </summary>
		/// <param name="x">left position.</param>
		/// <param name="y">right position.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: left postion goes after right position.</item>
		/// <item><c>false</c>: left postion is same or goes before right position.</item>
		/// </list>
		/// </returns>
		public static bool operator >(SourcePosition x, SourcePosition y)
		{
			return x.CompareTo(y) > 0;
		}

		/// <summary>
		/// Checks if both positions are the same.
		/// </summary>
		/// <param name="x">left position.</param>
		/// <param name="y">right position.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: both positions are the same.</item>
		/// <item><c>false</c>: positions differ from each other.</item>
		/// </list>
		/// </returns>
		public static bool operator ==(SourcePosition x, SourcePosition y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// Checks if both positions are not the same.
		/// </summary>
		/// <param name="x">left position.</param>
		/// <param name="y">right position.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: both positions are not the the same.</item>
		/// <item><c>false</c>: positions are the same.</item>
		/// </list>
		/// </returns>
		public static bool operator !=(SourcePosition x, SourcePosition y)
		{
			return !x.Equals(y);
		}

		/// <summary>
		/// Compares current postion with other one.
		/// </summary>
		/// <param name="other">Position to compare with.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: both positions are the same.</item>
		/// <item><c>false</c>: positions differ from each other.</item>
		/// </list>
		/// </returns>
		public bool Equals(SourcePosition other)
		{
			return Line == other.Line
				&& Column == other.Column;
		}

		/// <summary>
		/// Compares current postion with other one.
		/// </summary>
		/// <param name="obj">Position to compare with.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item><c>true</c>: both positions are the same.</item>
		/// <item><c>false</c>: positions differ from each other oth <paramref name="obj"/> is not an instance of <see cref="SourcePosition"/>.</item>
		/// </list>
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is SourcePosition otherSourcePosition) && Equals(otherSourcePosition);
		}

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode()
		{
			return Column.GetHashCode() ^ Column.GetHashCode();
		}

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
	}
}
