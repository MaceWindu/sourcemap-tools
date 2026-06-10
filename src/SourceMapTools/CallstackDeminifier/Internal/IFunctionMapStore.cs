using System.Collections.Generic;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>Internal API.</summary>
public interface IFunctionMapStore
{
	/// <summary>Internal API.</summary>
	public IReadOnlyList<FunctionMapEntry>? GetFunctionMapForSourceCode(string? sourceCodeUrl);
}