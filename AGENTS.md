# AGENTS.md — QaaS.Common.Assertions
Guidance for AI agents working in this repository.

## What this repo is
`QaaS.Common.Assertions` is a Tier-1 shared library that ships 11 concrete `IAssertion` hook
implementations consumed by `QaaS.Runner`. Every class inherits from
`BaseAssertion<TConfiguration>` (defined in `QaaS.Framework.SDK`) and is discovered at
runtime via Framework assembly scanning (scan order: `QaaS.*` → `Common.*` → user assemblies).
The package is published as `QaaS.Common.Assertions` (NuGet, net10.0, version prefix `0.0.0`,
tag-driven in CI) and consumed by Runner and user test projects.

## Projects / Layout

| Project | Purpose |
|---|---|
| `QaaS.Common.Assertions/` | Main library — 11 assertion hooks + config models |
| `QaaS.Common.Assertions.Tests/` | NUnit 4.x test project; coverage collected and reported |

Assertion families:
| Family | Classes |
|---|---|
| Hermeticity | `HermeticByExpectedOutputCount`, `HermeticByExpectedOutputCountInRange`, `HermeticByInputOutputPercentage`, `HermeticByInputOutputPercentageInRange`, `ValidateHermeticMetricsByInputOutputPercentage` |
| Delay / Latency | `DelayByAverage`, `DelayByChunks` |
| Content | `OutputContentByExpectedCsvResults` |
| Schema | `ObjectOutputJsonSchema` (drafts 06/07/2019-09/2020-12/next via `JsonSchema.Net`) |
| Deserialization | `OutputDeserializableTo` |
| HTTP Metadata | `HttpStatus` |

## Build & test
```
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```
CI runs on `windows-latest`. Coverage collected via `dotnet-coverage collect` and reported; no minimum threshold enforced.

## Critical gotchas
- **Assembly scanning contract**: hook classes MUST be public, non-abstract, and implement
  `IAssertion`. Namespace and assembly name must match `QaaS.*` or `Common.*` prefix patterns
  for the scanner to pick them up. Rename carefully.
- **BaseAssertion<TConfiguration>**: every assertion MUST inherit this generic base; direct
  `IAssertion` implementation bypasses config deserialization and will silently fail at runtime.
- **Tier-1 position**: this library sits between `QaaS.Framework.*` (Tier 0) and Runner/Mocker
  (Tier 2). Never add a dependency on Runner, Mocker, or any Tier-2+ package.
- **Key dependencies**: `JsonSchema.Net` (v9.1.3) for schema validation; `morelinq` (v4.4.0)
  for sequence operations; `QaaS.Framework.SDK` + `Configurations` + `Serialization` (v1.5.1).
- **Version pinning**: package version is `0.0.0` prefix; actual published version is injected
  from the Git tag in CI — do not hand-edit the version property.
- **CSV assertion**: `OutputContentByExpectedCsvResults` supports error-range, override-value,
  and base64-to-hex field-level validators — test each validator path when modifying.

## Process
- QaaS harness pipeline: plan → contract → implement → adversarial evaluation (rubric ≥7/10
  on Correctness / Completeness / Craft / Robustness).
- TDD: write failing tests first.
- Conventional commits: `feat:`, `fix:`, `chore(release):`, scoped forms like `fix(ci):`.
- `dotnet format` / `dotnet csharpier .` before every commit.