using System.Collections.Generic;
using NUnit.Framework;
using SourcemapToolkit.SourcemapParser;
using SourcemapTools.CallstackDeminifier.Internal;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests;

public class SourceMapExtensionsUnitTests
{
	[Test]
	public void GetDeminifiedMethodName_EmptyBinding_ReturnNullMethodName()
	{
		// Arrange
		var bindings = new List<BindingInformation>();
		var sourceMap = new SourceMapMock();

		// Act
		var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap, bindings);

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void GetDeminifiedMethodName_HasSingleBindingNoMatchingMapping_ReturnNullMethodName()
	{
		// Arrange
		var bindings = new List<BindingInformation>()
			{
				new(string.Empty, new SourcePosition(20, 15))
			};

		var sourceMap = new SourceMapMock((_, _) => null);

		// Act
		var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap, bindings);

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void GetDeminifiedMethodName_HasSingleBindingMatchingMapping_ReturnsMethodName()
	{
		// Arrange
		var bindings = new List<BindingInformation>()
			{
				new(string.Empty, new SourcePosition(5, 8))
			};

		var sourceMap = new SourceMapMock((x, def) => x is { Line: 5, Column: 8 } ? new(generatedSourcePosition: default, null, originalName: "foo", null) : def(x));

		// Act
		var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap, bindings);

		// Assert
		Assert.That(result, Is.EqualTo("foo"));
	}

	[Test]
	public void GetDeminifiedMethodName_MatchingMappingMultipleBindingsMissingPrototypeMapping_ReturnsMethodName()
	{
		// Arrange
		var bindings = new List<BindingInformation>
			{
				new(string.Empty, new SourcePosition(86, 52)),
				new(string.Empty, new SourcePosition(88, 78))
			};

		var sourceMap = new SourceMapMock((x, def)
			=> x is { Line: 86, Column: 52 }
				? null
				: x is { Line: 88, Column: 78 }
					? new MappingEntry(default, null, "baz", null)
					: def(x));

		// Act
		var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap, bindings);

		// Assert
		Assert.That(result, Is.EqualTo("baz"));
	}

	[Test]
	public void GetDeminifiedMethodName_MatchingMappingMultipleBindings_ReturnsMethodNameWithFullBinding()
	{
		// Arrange
		var bindings = new List<BindingInformation>
			{
				new(string.Empty, new SourcePosition(5, 5)),
				new(string.Empty, new SourcePosition(20, 10))
			};

		var sourceMap = new SourceMapMock((x, def)
	=> x is { Line: 5, Column: 5 }
		? new MappingEntry(generatedSourcePosition: default, null, originalName: "bar", null)
		: x is { Line: 20, Column: 10 }
			? new MappingEntry(generatedSourcePosition: default, null, originalName: "baz", null)
			: def(x));

		// Act
		var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap, bindings);

		// Assert
		Assert.That(result, Is.EqualTo("bar.baz"));
	}
}
