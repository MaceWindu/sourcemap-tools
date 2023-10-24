using System;
using SourcemapToolkit.CallstackDeminifier;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// This class only deminifies the method name in a stack frame. It does not depend on having a source map available during runtime.
/// </summary>
public sealed class MethodNameStackFrameDeminifier(IFunctionMapStore functionMapStore, IFunctionMapConsumer functionMapConsumer)
	: IStackFrameDeminifier
{
	private readonly IFunctionMapConsumer _functionMapConsumer = functionMapConsumer;
	private readonly IFunctionMapStore _functionMapStore = functionMapStore;

	/// <summary>
	/// This method will deminify the method name of a single stack from from a minified stack trace.
	/// </summary>
	StackFrameDeminificationResult IStackFrameDeminifier.DeminifyStackFrame(StackFrame stackFrame, string? callerSymbolName, bool preferSourceMapsSymbols)
	{
		if (stackFrame == null)
		{
			throw new ArgumentNullException(nameof(stackFrame));
		}

		var deminificationError = DeminificationError.None;

		FunctionMapEntry? wrappingFunction = null;

		// This code deminifies the stack frame by finding the wrapping function in
		// the generated code and then using the source map to find the name and
		// and original source location.
		var functionMap = _functionMapStore.GetFunctionMapForSourceCode(stackFrame.FilePath);
		if (functionMap != null)
		{
			wrappingFunction = _functionMapConsumer.GetWrappingFunctionForSourceLocation(stackFrame.SourcePosition, functionMap);

			if (wrappingFunction == null)
			{
				deminificationError = DeminificationError.NoWrappingFunctionFound;
			}
		}
		else
		{
			deminificationError = DeminificationError.NoSourceCodeProvided;
		}

		return new StackFrameDeminificationResult(
			deminificationError,
			new StackFrame(wrappingFunction?.DeminifiedMethodName));
	}
}