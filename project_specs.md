# project_specs.md — QaaS.Common.Assertions

> Architectural spec; see `CLAUDE.md` for the AI operating manual.

## Purpose

Distribute a curated set of pre-built `IAssertion` hooks for the QaaS
test runner: hermeticity checks, delay analysis, content / CSV / schema
validation, deserialisability checks, and HTTP status assertions.

## Surface (categorised)

- **Hermetic (5)** — input vs output count / percentage relationships.
- **Delay (2)** — average + per-chunk latency analysis.
- **Content (1)** — CSV field-by-field validation with pluggable field
  validators.
- **Schema (1)** — JSON Schema (drafts 06/07/2019-09/2020-12/next).
- **Deserialisation (1)** — verify items deserialise via the configured
  deserialiser.
- **HTTP (1)** — verify expected status code is present in outputs.

Total: 12 hooks. Discovered by `QaaS.Framework.Providers` via assembly
scanning.

## Configuration

Each assertion has a `record TConfig` with DataAnnotations attributes
(`[Required]`, `[Range]`, `[Description]`). Config is YAML-bound via
`QaaS.Framework.Configurations`.

## Build, packaging, CI

- Target: `.NET 10.0`.
- NuGet identity: `QaaS.Common.Assertions`.
- CI: standard pipeline (restore → build → test → coverage → pack-and-push
  on stable tags) with NuGet metadata validation (Description +
  PackageReadmeFile).
- Debug sources embedded, no `.snupkg`.

## Quality

- ≥ 70 % line coverage.
- Every assertion has at least one happy-path and one edge-case test.
- Public API: XML doc comments with `<qaas-docs>` linking to the docs
  site (`https://docs.qaas.online/assertions/...`).

## Compatibility

Tracks `QaaS.Framework.SDK` major version. Currently aligned with
1.4.2. Breaking shape changes require a coordinated rollout.
