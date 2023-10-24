using System;

namespace SourcemapToolkit.SourcemapParser;

/// <summary>
/// Identifies the location of a piece of code in a JavaScript file.
/// </summary>
/// <remarks>
/// Creates position instance with specified coordinates.
/// </remarks>
/// <param name="line">Zero-based position line number.</param>
/// <param name="column">Zero-based position column number.</param>
public readonly struct SourcePosition(int line, int column) : IComparable<SourcePosition>, IEquatable<SourcePosition>
{
	/// <summary>
	/// Unresolved stack frame position token.
	/// </summary>
	public static readonly SourcePosition NotFound = new(-1, -1);

	/// <summary>
	/// Zero-based position line number.
	/// </summary>
	public int Line { get; } = line;
	/// <summary>
	/// Zero-based position column number.
	/// </summary>
	public int Column { get; } = column;

	/// <summary>
	/// Compares current position instance with another position.
	/// </summary>
	/// <param name="other">Position to compare to.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item>-1: current position located before other position;</item>
	/// <item>0: both positions are the same;</item>
	/// <item>1: current position located after other position.</item>
	/// </list>
	/// </returns>
	public int CompareTo(SourcePosition other) => Line == other.Line ? Column.CompareTo(other.Column) : Line.CompareTo(other.Line);

	/// <summary>
	/// Checks if left position is going before right position.
	/// </summary>
	/// <param name="x">left position.</param>
	/// <param name="y">right position.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: left position goes before right position;</item>
	/// <item><c>false</c>: left position is same or goes after right position.</item>
	/// </list>
	/// </returns>
	public static bool operator <(SourcePosition x, SourcePosition y) => x.CompareTo(y) < 0;

	/// <summary>
	/// Checks if left position is going after right position.
	/// </summary>
	/// <param name="x">left position.</param>
	/// <param name="y">right position.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: left position goes after right position;</item>
	/// <item><c>false</c>: left position is same or goes before right position.</item>
	/// </list>
	/// </returns>
	public static bool operator >(SourcePosition x, SourcePosition y) => x.CompareTo(y) > 0;

	/// <summary>
	/// Checks if both positions are the same.
	/// </summary>
	/// <param name="x">left position.</param>
	/// <param name="y">right position.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both positions are the same;</item>
	/// <item><c>false</c>: positions differ from each other.</item>
	/// </list>
	/// </returns>
	public static bool operator ==(SourcePosition x, SourcePosition y) => x.Equals(y);

	/// <summary>
	/// Checks if both positions are not the same.
	/// </summary>
	/// <param name="x">left position.</param>
	/// <param name="y">right position.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both positions are not the the same;</item>
	/// <item><c>false</c>: positions are the same.</item>
	/// </list>
	/// </returns>
	public static bool operator !=(SourcePosition x, SourcePosition y) => !x.Equals(y);

	/// <summary>
	/// Compares current position with other one.
	/// </summary>
	/// <param name="other">Position to compare with.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both positions are the same;</item>
	/// <item><c>false</c>: positions differ from each other.</item>
	/// </list>
	/// </returns>
	public bool Equals(SourcePosition other) => Line == other.Line && Column == other.Column;

	/// <summary>
	/// Compares current position with other one.
	/// </summary>
	/// <param name="obj">Position to compare with.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both positions are the same;</item>
	/// <item><c>false</c>: positions differ from each other oth <paramref name="obj"/> is not an instance of <see cref="SourcePosition"/>.</item>
	/// </list>
	/// </returns>
	public override bool Equals(object obj) => (obj is SourcePosition otherSourcePosition) && Equals(otherSourcePosition);

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => Column.GetHashCode() ^ Column.GetHashCode();

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

	/// <summary>
	/// Checks if left position is same or going before right position.
	/// </summary>
	/// <param name="left">left position.</param>
	/// <param name="right">right position.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: left position is same or goes before right position;</item>
	/// <item><c>false</c>: left position is same or goes after right position.</item>
	/// </list>
	/// </returns>
	public static bool operator <=(SourcePosition left, SourcePosition right) => left.CompareTo(right) <= 0;

	/// <summary>
	/// Checks if left position is same or going after right position.
	/// </summary>
	/// <param name="left">left position.</param>
	/// <param name="right">right position.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: left position is same or goes after right position;</item>
	/// <item><c>false</c>: left position goes before right position.</item>
	/// </list>
	/// </returns>
	public static bool operator >=(SourcePosition left, SourcePosition right) => left.CompareTo(right) >= 0;
}
