using System.Collections.Generic;
using Moq;
using SourcemapToolkit.SourcemapParser;
using Xunit;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{
	public class SourceMapExtensionsUnitTests
	{
		[Fact]
		public void GetDeminifiedMethodName_EmptyBinding_ReturnNullMethodName()
		{
			// Arrange
			var bindings = new List<BindingInformation>();
			var sourceMap = CreateSourceMapMock();

			// Act
			var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap.Object, bindings);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public void GetDeminifiedMethodName_HasSingleBindingNoMatchingMapping_ReturnNullMethodName()
		{
			// Arrange
			var bindings = new List<BindingInformation>()
				{
					new BindingInformation(
						name: default!,
						sourcePosition: new SourcePosition(20, 15))
				};

			var sourceMap = CreateSourceMapMock();
			sourceMap.Setup(x => x.GetMappingEntryForGeneratedSourcePosition(It.IsAny<SourcePosition>())).Returns((MappingEntry?)null);

			// Act
			var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap.Object, bindings);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public void GetDeminifiedMethodName_HasSingleBindingMatchingMapping_ReturnsMethodName()
		{
			// Arrange
			var bindings = new List<BindingInformation>()
				{
					new BindingInformation(
						name: default!,
						sourcePosition: new SourcePosition(5, 8))
				};

			var sourceMap = CreateSourceMapMock();
			sourceMap.Setup(
				x =>
					x.GetMappingEntryForGeneratedSourcePosition(
						It.Is<SourcePosition>(y => y.Line == 5 && y.Column == 8)))
				.Returns(new MappingEntry(generatedSourcePosition: default, null, originalName: "foo", null));

			// Act
			var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap.Object, bindings);

			// Assert
			Assert.Equal("foo", result);
			sourceMap.Verify();
		}

		[Fact]
		public void GetDeminifiedMethodName_MatchingMappingMultipleBindingsMissingPrototypeMapping_ReturnsMethodName()
		{
			// Arrange
			var bindings = new List<BindingInformation>
				{
					new BindingInformation(
						name: default!,
						sourcePosition: new SourcePosition(86, 52)),
					new BindingInformation(
						name: default!,
						sourcePosition: new SourcePosition(88, 78))
				};

			var sourceMap = CreateSourceMapMock();
			sourceMap.Setup(
				x =>
					x.GetMappingEntryForGeneratedSourcePosition(
						It.Is<SourcePosition>(y => y.Line == 86 && y.Column == 52)))
				.Returns((MappingEntry?)null);

			sourceMap.Setup(
				x =>
					x.GetMappingEntryForGeneratedSourcePosition(
						It.Is<SourcePosition>(y => y.Line == 88 && y.Column == 78)))
				.Returns(new MappingEntry(default, null, "baz", null));

			// Act
			var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap.Object, bindings);

			// Assert
			Assert.Equal("baz", result);
			sourceMap.Verify();
		}

		[Fact]
		public void GetDeminifiedMethodName_MatchingMappingMultipleBindings_ReturnsMethodNameWithFullBinding()
		{
			// Arrange
			var bindings = new List<BindingInformation>
				{
					new BindingInformation(
						name: default!,
						sourcePosition: new SourcePosition(5, 5)),
					new BindingInformation(
						name: default!,
						sourcePosition: new SourcePosition(20, 10))
				};

			var sourceMap = CreateSourceMapMock();
			sourceMap.Setup(
				x =>
					x.GetMappingEntryForGeneratedSourcePosition(
						It.Is<SourcePosition>(y => y.Line == 5 && y.Column == 5)))
				.Returns(new MappingEntry(generatedSourcePosition: default, null, originalName: "bar", null));

			sourceMap.Setup(
				x =>
					x.GetMappingEntryForGeneratedSourcePosition(
						It.Is<SourcePosition>(y => y.Line == 20 && y.Column == 10)))
				.Returns(new MappingEntry(generatedSourcePosition: default, null, originalName: "baz", null));

			// Act
			var result = SourceMapExtensions.GetDeminifiedMethodName(sourceMap.Object, bindings);

			// Assert
			Assert.Equal("bar.baz", result);
			sourceMap.Verify();
		}

		private static Mock<SourceMap> CreateSourceMapMock()
		{
			return new Mock<SourceMap>(() => new SourceMap(
				0 /* version */,
				default /* file */,
				default /* mappings */,
				default /* sources */,
				default /* names */,
				new List<MappingEntry>() /* parsedMappings */,
				default /* sourcesContent */));
		}
	}
}
