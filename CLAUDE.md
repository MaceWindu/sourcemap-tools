# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

C# library (`SourceMapTools` NuGet package) for parsing JavaScript source maps and deminifying JS call stacks. Fork of [microsoft/sourcemap-toolkit](https://github.com/microsoft/sourcemap-toolkit), maintained to add modern .NET / ES6+ support and resume NuGet publishing.

## Commands

- Build: `dotnet build SourcemapTools.slnx`
- Test (all frameworks): `dotnet test SourcemapTools.slnx`
- Run a single test: `dotnet test --filter "FullyQualifiedName~SourceMapParserUnitTests"` (or `~MethodName`)
- Pack: `dotnet pack src/SourceMapTools/SourcemapTools.csproj -c Release -o .build/nuget`
- The library targets `netstandard2.0`; tests run on `net462;net8.0;net9.0;net10.0`.

`global.json` pins the .NET 10 SDK (`rollForward: latestFeature`) and selects the **Microsoft.Testing.Platform** test runner — `dotnet test` drives MTP, not VSTest, so the test project is `OutputType=Exe` with `EnableNUnitRunner`. The solution is the XML-based `.slnx` format (no `.sln`).

Warnings are errors (`TreatWarningsAsErrors`, `WarningLevel 999`, `AnalysisLevel preview-All`, code style enforced in build). The full AsyncFixer and Meziantou.Analyzer rule sets are enabled as errors in `.editorconfig`; a handful are turned off there with inline rationale (e.g. MA0009/MA0023 — regexes use numbered capture groups; MA0104 — `StackFrame` shadows `System.Diagnostics.StackFrame`; MA0181 — necessary esprima cast). `tests/.editorconfig` relaxes test-only rules. Expect builds to fail on analyzer/style violations, not just compile errors. XML doc comments are required on public members (`GenerateDocumentationFile`).

The library targets only `netstandard2.0`, so modern BCL APIs are filled in by **Meziantou.Polyfill** (not PolySharp). The set of generated polyfills is an explicit allow-list (`MeziantouPolyfill_IncludedPolyfills`) in `src/SourceMapTools/SourcemapTools.csproj` — when you use a newer API and the ns2.0 build fails with a missing type/member, add its `T:`/`M:` id there.

The assembly is strong-name signed; `SourceMapTools.snk` must be present at repo root or builds fail.

## Architecture

Two cooperating subsystems under `src/SourceMapTools/`, split by namespace (note the inconsistent `SourcemapToolkit` vs `SourcemapTools` namespace roots — preserve whatever an existing file uses):

### SourcemapParser/ — source map model + (de)serialization
- `SourceMapParser.ParseSourceMap(Stream)` reads JSON into a `SourceMap`. `SourceMapGenerator.SerializeMapping` writes it back. Uses `System.Text.Json`.
- `SourceMap` holds `ParsedMappings` (a sorted `IReadOnlyList<MappingEntry>`). Lookups go through `GetMappingEntryForGeneratedSourcePosition`, which **binary-searches** by generated position and falls back to the nearest-smaller entry (`SourcePosition.IsEqualish`) — keep `ParsedMappings` sorted by generated position.
- `ApplySourceMap` chains maps (c→b + b→a ⇒ c→a) by rewriting each mapping through a submap.
- `SourceMapTransformer` produces a reduced map. The raw VLQ `mappings` string is decoded/encoded via `Internal/Base64Vlq*` and `MappingsListParser`; `NumericMappingEntry` is the compact intermediate form.
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

`tests/SourcemapTools.UnitTests/` uses **NUnit** (via the Microsoft.Testing.Platform runner) with hand-written mock classes in `Mocks/`. Test folders mirror the `src` layout. End-to-end tests (`*EndToEndTests.cs`) run real fixtures including a webpack bundle under `webpackapp/` and Closure-compiled samples; these `.js`/`.map` files are copied to output via the csproj.

## CI / Release

Azure DevOps pipeline in `ci/default.yml`. The package is produced with `dotnet pack` (SDK-style packaging — metadata lives in `SourcemapTools.csproj`, there is no `.nuspec`); the version is passed via `-p:Version` (`packageVersion` for `release`, a `-rc.<build>` suffix for `master`). Debug symbols are embedded in the assembly (`DebugType=embedded`) — no separate `.snupkg`. Pushes to `master` publish RC packages to Azure Artifacts; pushes to `release` publish to NuGet.org. Package dependencies are managed centrally in `Directory.Packages.props`.
