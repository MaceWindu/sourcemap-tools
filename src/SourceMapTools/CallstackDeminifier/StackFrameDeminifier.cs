﻿using System;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Class responsible for deminifying a single stack frame in a minified stack trace.
	/// This method of deminification relies on a source map being available at runtime.
	/// Since source maps take up a large amount of memory, this class consumes considerably 
	/// more memory than SimpleStackFrame Deminifier during runtime.
	/// </summary>
	internal class StackFrameDeminifier : IStackFrameDeminifier
	{
		private readonly ISourceMapStore _sourceMapStore;
		private readonly IStackFrameDeminifier? _methodNameDeminifier;

		public StackFrameDeminifier(ISourceMapStore sourceMapStore)
		{
			_sourceMapStore = sourceMapStore;
		}

		public StackFrameDeminifier(ISourceMapStore sourceMapStore, IFunctionMapStore functionMapStore, IFunctionMapConsumer functionMapConsumer) : this(sourceMapStore)
		{
			_methodNameDeminifier = new MethodNameStackFrameDeminifier(functionMapStore, functionMapConsumer);
		}

		/// <summary>
		/// This method will deminify a single stack from from a minified stack trace.
		/// </summary>
		/// <returns>Returns a StackFrameDeminificationResult that contains a stack trace that has been translated to the original source code. The DeminificationError Property indicates if the StackFrame could not be deminified. DeminifiedStackFrame will not be null, but any properties of DeminifiedStackFrame could be null if the value could not be extracted. </returns>
		StackFrameDeminificationResult IStackFrameDeminifier.DeminifyStackFrame(StackFrame stackFrame, string? callerSymbolName)
		{
			if (stackFrame == null)
			{
				throw new ArgumentNullException(nameof(stackFrame));
			}

			var sourceMap = _sourceMapStore.GetSourceMapForUrl(stackFrame.FilePath);
			var generatedSourcePosition = stackFrame.SourcePosition;

			StackFrameDeminificationResult? result = null;
			if (_methodNameDeminifier != null)
			{
				result = _methodNameDeminifier.DeminifyStackFrame(stackFrame, callerSymbolName);
			}

			if (result == null || result.DeminificationError == DeminificationError.NoSourceCodeProvided)
			{
				result = new StackFrameDeminificationResult(
					DeminificationError.None,
					new StackFrame(callerSymbolName));
			}

			if (result.DeminificationError == DeminificationError.None)
			{
				var generatedSourcePositionMappingEntry =
					sourceMap?.GetMappingEntryForGeneratedSourcePosition(generatedSourcePosition);

				if (generatedSourcePositionMappingEntry == null)
				{
					if (sourceMap == null)
					{
						result.DeminificationError = DeminificationError.NoSourceMap;
					}
					else
					{
						result.DeminificationError = DeminificationError.NoMatchingMapingInSourceMap;
					}
				}
				else
				{
					result.DeminifiedStackFrame.FilePath = generatedSourcePositionMappingEntry.OriginalFileName;
					result.DeminifiedStackFrame.SourcePosition = generatedSourcePositionMappingEntry.OriginalSourcePosition;
					result.DeminifiedSymbolName = generatedSourcePositionMappingEntry.OriginalName;
				}
			}

			return result;
		}
	}
}
