using System;
using System.Diagnostics;

namespace SourcemapToolkit.SourcemapParser;

/// <summary>
/// Source map entry.
/// </summary>
/// <remarks>
/// Creates source map entry instance.
/// </remarks>
/// <param name="generatedSourcePosition">Entry position in minified script.</param>
/// <param name="originalSourcePosition">Entry position in original script.</param>
/// <param name="originalName">Original name of source map entry.</param>
/// <param name="originalFileName">File name of original source file with entry code.</param>
[DebuggerDisplay("(Column: {GeneratedSourcePosition.Column}, Line: {GeneratedSourcePosition.Line}): {OriginalName}")]
public readonly struct MappingEntry(
	SourcePosition generatedSourcePosition,
	SourcePosition? originalSourcePosition,
	string? originalName,
	string? originalFileName) : IEquatable<MappingEntry>
{
	/// <summary>
	/// Creates source map entry instance.
	/// </summary>
	/// <param name="generatedSourcePosition">Entry position in minified script.</param>
	public MappingEntry(SourcePosition generatedSourcePosition)
		: this(generatedSourcePosition, null, null, null)
	{
	}

	/// <summary>
	/// The location of the line of code in the transformed code.
	/// </summary>
	public readonly SourcePosition GeneratedSourcePosition { get; } = generatedSourcePosition;

	/// <summary>
	/// The location of the code in the original source code.
	/// </summary>
	public readonly SourcePosition OriginalSourcePosition { get; } = originalSourcePosition ?? SourcePosition.NotFound;

	/// <summary>
	/// The original name of the code referenced by this mapping entry.
	/// </summary>
	public readonly string? OriginalName { get; } = originalName;

	/// <summary>
	/// The name of the file that originally contained this code.
	/// </summary>
	public readonly string? OriginalFileName { get; } = originalFileName;

	/// <summary>
	/// Returns copy of entry with source positions having zero as column number.
	/// </summary>
	/// <returns>Returns copy of current entry.</returns>
	public MappingEntry CloneWithResetColumnNumber() => new(
		new SourcePosition(GeneratedSourcePosition.Line, 0),
		new SourcePosition(OriginalSourcePosition.Line, 0),
		OriginalName,
		OriginalFileName);

	/// <summary>
	/// Compares current mapping entry with another one.
	/// </summary>
	/// <param name="anEntry">Mapping entry to compare with.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both entries are the same;</item>
	/// <item><c>false</c>: entries differ from each other.</item>
	/// </list>
	/// </returns>
	public bool IsValueEqual(MappingEntry anEntry) => OriginalName == anEntry.OriginalName &&
			OriginalFileName == anEntry.OriginalFileName &&
			GeneratedSourcePosition.Equals(anEntry.GeneratedSourcePosition) &&
			OriginalSourcePosition.Equals(anEntry.OriginalSourcePosition);

	/// <summary>
	/// Compares current mapping entry with another one.
	/// </summary>
	/// <param name="obj">Mapping entry to compare with.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both entries are the same;</item>
	/// <item><c>false</c>: entries differ from each other oth <paramref name="obj"/> is not an instance of <see cref="MappingEntry"/>.</item>
	/// </list>
	/// </returns>
	public override bool Equals(object? obj) => obj is MappingEntry mappingEntry && Equals(mappingEntry);

	/// <summary>
	/// An implementation of Josh Bloch's hashing algorithm from Effective Java.
	/// It is fast, offers a good distribution (with primes 23 and 31), and allocation free.
	/// </summary>
	public override int GetHashCode()
	{
		unchecked // Wrap to protect overflow
		{
			var hash = 23;
			hash = (hash * 31) + GeneratedSourcePosition.GetHashCode();
			hash = (hash * 31) + OriginalSourcePosition.GetHashCode();
			hash = (hash * 31) + (OriginalName ?? "").GetHashCode();
			hash = (hash * 31) + (OriginalFileName ?? "").GetHashCode();
			return hash;
		}
	}

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(MappingEntry other) =>
		OriginalName == other.OriginalName &&
		OriginalFileName == other.OriginalFileName &&
		GeneratedSourcePosition.Equals(other.GeneratedSourcePosition) &&
		OriginalSourcePosition.Equals(other.OriginalSourcePosition);

	/// <summary>
	/// Checks if both <see cref="MappingEntry"/> are the same.
	/// </summary>
	/// <param name="left">Left object.</param>
	/// <param name="right">Right object.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: both objects are the same;</item>
	/// <item><c>false</c>: objects differ from each other.</item>
	/// </list>
	/// </returns>
	public static bool operator ==(MappingEntry left, MappingEntry right) => left.Equals(right);

	/// <summary>
	/// Checks if both <see cref="MappingEntry"/> are not the same.
	/// </summary>
	/// <param name="left">Left object.</param>
	/// <param name="right">Right object.</param>
	/// <returns>
	/// <list type="bullet">
	/// <item><c>true</c>: objects differ from each other;</item>
	/// <item><c>false</c>: both objects are the same.</item>
	/// </list>
	/// </returns>
	public static bool operator !=(MappingEntry left, MappingEntry right) => !(left == right);
}