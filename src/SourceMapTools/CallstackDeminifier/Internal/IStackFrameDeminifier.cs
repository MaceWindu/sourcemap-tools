﻿using SourcemapToolkit.CallstackDeminifier;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Internal API.
/// </summary>
public interface IStackFrameDeminifier
{
	/// <summary>
	/// This method will deminify a single stack from from a minified stack trace.
	/// </summary>
	/// <param name="stackFrame">Stack frame descriptor.</param>
	/// <param name="callerSymbolName">Caller's symbol name.</param>
	/// <param name="preferSourceMapsSymbols">if true, we will use exact sourcemap names directly for deobfuscation, without guessing the wrapper function name from source code.</param>
	/// <returns>Returns a StackFrameDeminificationResult that contains a stack trace that has been translated to the original source code. The DeminificationError Property indicates if the StackFrame could not be deminified. DeminifiedStackFrame will not be null, but any properties of DeminifiedStackFrame could be null if the value could not be extracted. </returns>
	StackFrameDeminificationResult DeminifyStackFrame(StackFrame stackFrame, string? callerSymbolName, bool preferSourceMapsSymbols);
}