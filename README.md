# Source Map Tools [![Build Status](https://img.shields.io/azure-devops/build/macewindu/projects/1/master?label=build%20(master))](https://dev.azure.com/macewindu/projects/_build/latest?definitionId=1&branchName=master) [![NuGet](https://img.shields.io/nuget/v/SourceMapTools.svg)](https://www.nuget.org/packages/SourceMapTools/) [![License](https://img.shields.io/github/license/MaceWindu/sourcemap-tools)](LICENSE.txt)

This is a C# library for working with JavaScript source maps and deminifying JavaScript callstacks.

This is a fork of [microsoft/sourcemap-toolkit](https://github.com/microsoft/sourcemap-toolkit) project that solves following outstanding issues with original project:

- no active development is done anymore for original project, mostly support-only changes for use inside MS teams
- no nuget publishing for recent changes: [#64](https://github.com/microsoft/sourcemap-toolkit/issues/64)
- lack of support for modern frameworks (.net core): [#57](https://github.com/microsoft/sourcemap-toolkit/issues/57)
- lack of support for ES6+: [#66](https://github.com/microsoft/sourcemap-toolkit/issues/66)

## Source Map Parsing
The `SourcemapTools.dll` provides an API for parsing a source map into an object that is easy to work with and an API for serializing source map object back to json string.
The source map class has a method `GetMappingEntryForGeneratedSourcePosition`, which can be used to find a source map mapping entry that likely corresponds to a piece of generated code.

### Usage
The top level API for source map parsing is the `SourceMapParser.ParseSourceMap` method. The input is a `Stream` that can be used to access the contents of the source map.
The top level API for source map serializing is the `SourceMapGenerator.SerializeMapping` method. The input is a `SourceMap` that to be serialized and an optional JsonSerializerSettings that can be used to control the json serialization.
A sample usage of the library is shown below.

```csharp
// Parse the source map from file
SourceMap sourceMap;
using (var stream = new FileStream(@"sample.sourcemap", FileMode.Open))
{
    sourceMap = SourceMapParser.ParseSourceMap(stream);
}

// Manipulate the source map
...

// Save to source map to file
string serializedMap = SourceMapGenerator.SerializeMapping(sourceMap);
File.WriteAllText(@"updatedSample.sourcemap", serializedMap);
```

### Chaining source maps
A common use case when dealing with source maps is multiple mapping layers. You can use `ApplySourceMap` to chain maps together to link back to the source

```cs
var inOriginal = new SourcePosition(34, 23);
var inBundled  = new SourcePosition(23, 12);
var inMinified = new SourcePosition(3 , 2 );

var originalToBundledEntry = new MappingEntry(inBundled, inOriginal, null, "original.js");
var bundledToMinifiedEntry = new MappingEntry(inMinified, inBundled, null, "bundle.js");

var bundledToOriginal = new SourceMap()
{
  File = "bundled.js",
  Sources = new List<string> { "original.js" },
  ParsedMappings = new List<MappingEntry> { originalToBundledEntry }
}

var minifiedToBundled = new SourceMap()
{
  File = "bundled.min.js",
  Sources = new List<string> { "bundled.js" },
  ParsedMappings = new List<MappingEntry> { bundledToMinifiedEntry }
}

// will contain mapping for line 3, column 2 in the minified file to line 34, column 23 in the original file
var minifiedToOriginal = minifiedToBundled.ApplySourceMap(bundledToOriginal);
```

## Call Stack Deminification
The `SourcemapToolkit.dll` allows for the deminification of JavaScript call stacks.
### Example
#### Call stack string
```
TypeError: Cannot read property 'length' of undefined
    at i (http://localhost:11323/crashcauser.min.js:1:113)
    at t (http://localhost:11323/crashcauser.min.js:1:75)
    at n (http://localhost:11323/crashcauser.min.js:1:50)
    at causeCrash (http://localhost:11323/crashcauser.min.js:1:222)
    at HTMLButtonElement.<anonymous> (http://localhost:11323/crashcauser.min.js:1:326)
```
#### Sample Minified `StackFrame` entry
```
    FilePath: "http://localhost:11323/crashcauser.min.js"
    MethodName: "i"
    SourcePosition.Column: 49
    SourcePosition.Line: 0
```
#### Sample Deminified `StackFrame` entry
```
    FilePath: "crashcauser.js"
    MethodName: "level1"
    SourcePosition.Column: 8
    SourcePosition.Line: 5
```
### Usage
The top level API for call stack deminification is the `StackTraceDeminifier.DeminifyStackTrace` method. For each url that appears in a JavaScript callstack, the library requires the contents of the JavaScript file and corresponding source map in order to determine the original method name and code location. This information is provided by the consumer of the API by implementing the `ISourceMapProvider` and `ISourceCodeProvider` interfaces. These interfaces are expected to return a `Stream` that can be used to access the contents of the requested JavaScript code or corresponding source map. A `StackTraceDeminifier` can be instantiated using one of the methods on `StackTraceDeminfierFactory`. A sample usage of the library is shown below.

```csharp
var sourceMapCallstackDeminifier = StackTraceDeminifierFactory.GetStackTraceDeminfier(new SourceMapProvider(), new SourceCodeProvider());
var deminifyStackTraceResult     = sourceMapCallstackDeminifier.DeminifyStackTrace(callstack, false);
var deminifiedCallstack          = deminifyStackTraceResult.ToString();
```

The result of `DeminifyStackTrace` is a `DeminifyStackTraceResult`, which is an object that contains a list of `StackFrameDeminificationResults` which contains the parsed minified `StackFrame` objects in the `MinifiedStackFrame` property and an enum indicating if any errors occurred when attempting to deminify the `StackFrame`. The `DeminifiedStackFrame` property contains the best guess `StackFrame` object that maps to the `MinifiedStackFrame` element with the same index. Note that any of the properties on a `StackTrace` object may be null if no value could be extracted from the input callstack string or source map.

#### Memory Consumption
Parsed source maps can take up a lot of memory for large JavaScript files. In order to allow for the `StackTraceDeminifier` to be used on servers with limited memory resources, the `StackTraceDeminfierFactory` exposes a `GetMethodNameOnlyStackTraceDeminfier` method that returns a `StackTraceDeminifier` that does not keep source maps in memory. Since the `StackTraceDeminifier` returned from this method only reads the source map once, the deminified stack frames will only contain the deminified method name and will not contain the original source location.

## Remarks
Browsers return one based line and column numbers, while the source map spec calls for zero based line and column numbers. In order to minimize confusion, line and column numbers are normalized to be zero based throughout the library.

## Acknowledgements
The Base64 VLQ decoding code was based on the implementation in the [Closure Compiler](https://github.com/google/closure-compiler/blob/master/src/com/google/debugging/sourcemap/Base64VLQ.java) which is licensed under the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0).

The source map parsing implementation and the relevant comments were based on the [Source Maps V3 spec](https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/mobilebasic?pref=2&pli=1) which is licensed under a [Creative Commons Attribution-ShareAlike 3.0 Unported License](https://creativecommons.org/licenses/by-sa/3.0/).

The source map parser uses [System.Text.Json](https://www.nuget.org/packages/System.Text.Json) which is licensed under the [MIT License](https://github.com/dotnet/runtime/blob/master/LICENSE.TXT).

The call stack deminifier use [Esprima .NET](https://www.nuget.org/packages/esprima) which is licensed under the [BSD 3-Clause License](https://github.com/sebastienros/esprima-dotnet/blob/dev/LICENSE.txt).

The unit tests for this library leverage the functionality provided by [Moq](https://www.nuget.org/packages/Moq). Moq is Open Source and released under the [BSD 3-Clause License](https://github.com/moq/moq4/blob/main/License.txt).

## License

Licensed under the [MIT](LICENSE.txt) License.
