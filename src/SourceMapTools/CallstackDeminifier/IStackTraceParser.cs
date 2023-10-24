using System.Collections.Generic;

namespace SourcemapToolkit.CallstackDeminifier;

/// <summary>
/// Stack trace parser contract.
/// </summary>
public interface IStackTraceParser
{
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
	/// This override drops the Message out parameter for backward compatibility.
	/// </remarks>
	IReadOnlyList<StackFrame> ParseStackTrace(string stackTraceString);

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
#pragma warning disable CA1021 // Avoid out parameters
	IReadOnlyList<StackFrame> ParseStackTrace(string stackTraceString, out string? message);
#pragma warning restore CA1021 // Avoid out parameters
}