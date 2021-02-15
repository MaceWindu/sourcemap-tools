using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
	/// <summary>
	/// Describes information regarding a binding that can be used for minification.
	/// Examples include methods, functions, and object declarations.
	/// </summary>
	internal struct BindingInformation
	{
		public BindingInformation(string name, SourcePosition sourcePosition)
		{
			Name = name;
			SourcePosition = sourcePosition;
		}
		/// <summary>
		/// The name of the method or class
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The location of the function name or class declaration
		/// </summary>
		public readonly SourcePosition SourcePosition;
	}
}
