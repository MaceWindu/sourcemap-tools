using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// Describes information regarding a binding that can be used for minification.
/// Examples include methods, functions, and object declarations.
/// </summary>
/// <param name="Name">The name of the method or class.</param>
/// <param name="SourcePosition">The location of the function name or class declaration.</param>
public readonly record struct BindingInformation(string Name, SourcePosition SourcePosition);
