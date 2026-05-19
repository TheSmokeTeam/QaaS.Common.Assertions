# project_specs.md — QaaS.Common.Assertions (package project)

The single package project: every concrete `IAssertion` lives here,
grouped by category folder.

## Folders

- `Hermetic/` — count / percentage relationships.
- `Delay/` — latency analysis.
- `Content/` — CSV validators + field-validator pluggable framework.
- `Schema/` — JSON Schema validators.
- `Deserialization/` — deserialisability assertions.
- `Http/` — HTTP status / metadata.
- `ConfigurationObjects/` — `record` configs per assertion.

## Forbidden in this project

- Adding non-assertion classes (generators / probes / processors) — wrong
  repo.
- Mutating shared state across `Assert()` calls.

## Tests live in

`QaaS.Common.Assertions.Tests` (sibling project).
