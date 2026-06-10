# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

C# library (`SourceMapTools` NuGet package) for parsing JavaScript source maps and deminifying JS call stacks. Fork of [microsoft/sourcemap-toolkit](https://github.com/microsoft/sourcemap-toolkit), maintained to add modern .NET / ES6+ support and resume NuGet publishing.

## Commands

- Build: `dotnet build SourcemapTools.sln`
- Test (all frameworks): `dotnet test SourcemapTools.sln`
- Run a single test: `dotnet test --filter "FullyQualifiedName~SourceMapParserUnitTests"` (or `~MethodName`)
- The library targets `netstandard2.0`; tests run on `net462;net6.0;net8.0`.

Warnings are errors (`TreatWarningsAsErrors`, `WarningLevel 999`, `AnalysisLevel preview-All`, code style enforced in build). Expect builds to fail on analyzer/style violations, not just compile errors. XML doc comments are required on public members (`GenerateDocumentationFile`).

The assembly is strong-name signed; `SourceMapTools.snk` must be present at repo root or builds fail.

## Architecture

Two cooperating subsystems under `src/SourceMapTools/`, split by namespace (note the inconsistent `SourcemapToolkit` vs `SourcemapTools` namespace roots — preserve whatever an existing file uses):

### SourcemapParser/ — source map model + (de)serialization
- `SourceMapParser.ParseSourceMap(Stream)` reads JSON into a `SourceMap`. `SourceMapGenerator.SerializeMapping` writes it back. Uses `System.Text.Json`.
- `SourceMap` holds `ParsedMappings` (a sorted `IReadOnlyList<MappingEntry>`). Lookups go through `GetMappingEntryForGeneratedSourcePosition`, which **binary-searches** by generated position and falls back to the nearest-smaller entry (`SourcePosition.IsEqualish`) — keep `ParsedMappings` sorted by generated position.
- `ApplySourceMap` chains maps (c→b + b→a ⇒ c→a) by rewriting each mapping through a submap.
- `SourceMapTransformer` produces a reduced map. The raw VLQ `mappings` string is decoded/encoded via `Internal/Base64Vlq*` and `MappingListParser`; `NumericMappingEntry` is the compact intermediate form.
- Line/column numbers are **zero-based throughout** (browsers report one-based — normalize at the boundary).

### CallstackDeminifier/ — turn minified stack traces back into original symbols
- Entry point: `StackTraceDeminifierFactory` builds a `StackTraceDeminifier` in one of three modes:
  - `GetStackTraceDeminifier` — full: needs both source maps and JS source; resolves method names + original positions; caches maps (high memory).
  - `GetMapOnlyStackTraceDeminifier` — source maps only, ES2015+ compatible, no JS file needed.
  - `GetMethodNameOnlyStackTraceDeminifier` — low memory; reads each map once, method names only, no original position.
- Consumers implement `ISourceMapProvider` / `ISourceCodeProvider` to feed file/map contents by URL.
- Pipeline: `StackTraceParser` parses the raw stack string → `StackFrame`s; `StackFrameDeminifier` maps each frame using the `SourceMap` plus a `FunctionMap`. The function map (which minified function spans which name) is built by walking the JS AST via **Esprima .NET** — see `FunctionMapGenerator`, `FunctionFinderVisitor`, `AstVisitorWithStack`. `SourceMapStore` / `FunctionMapStore` provide caching (`KeyValueCache`).

`Internal/` namespaces are implementation detail. A few types (`SourceMap`) are `class`/`virtual` purely so tests can mock them — comments in code flag this.

## Tests

`tests/SourcemapTools.UnitTests/` uses **NUnit** with hand-written mock classes in `Mocks/` (the README's mention of Moq is stale). Test folders mirror the `src` layout. End-to-end tests (`*EndToEndTests.cs`) run real fixtures including a webpack bundle under `webpackapp/` and Closure-compiled samples; these `.js`/`.map` files are copied to output via the csproj.

## CI / Release

Azure DevOps pipeline in `ci/default.yml`. Versions live in `ci/default.yml` (`assemblyVersion` / `packageVersion`); `ci/SetVersion.ps1` stamps `Directory.Build.props` and `ci/BuildNuspecs.ps1` updates the nuspec. Pushes to `master` publish RC packages to Azure Artifacts; pushes to `release` publish to NuGet.org. Package dependencies are managed centrally in `Directory.Packages.props`.
