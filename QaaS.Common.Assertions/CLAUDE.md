# CLAUDE.md — QaaS.Common.Assertions (library)

> Project-level operating manual. See repo root `CLAUDE.md` and
> `project_specs.md`.

## Purpose

Reusable `IAssertion` implementations (from `QaaS.Framework.SDK`) for
QaaS test workflows. Grouped by concern: hermeticity, delay, content,
schema, deserialization, HTTP.

## Source folders

- `Hermetic/` — `HermeticByExpectedOutputCount`,
  `HermeticByExpectedOutputCountInRange`,
  `HermeticByInputOutputPercentage`,
  `HermeticByInputOutputPercentageInRange`,
  `ValidateHermeticMetricsByInputOutputPercentage` (see
  `Hermetic/HermeticByExpectedOutputCount.cs`).
- `Delay/` — `DelayByAverage`, `DelayByChunks`.
- `ContentLogic/` — `OutputContentByExpectedCsvResults` with
  exact / error-range / override / base64-hex field validators.
- `SchemaLogic/` — `ObjectOutputJsonSchema` (drafts 06 / 07 / 2019-09 /
  2020-12).
- `DeserializationLogic/` — `OutputDeserializableTo`.
- `HttpMetaDataLogic/` — `HttpStatus`.
- `CommonAssertionsConfigs/` — DataAnnotations-decorated config records.

## Conventions

- Each assertion is a class implementing the SDK's `IAssertion`
  contract; configs are records with validation attributes.
- JSON Schema validation goes through `Json.Schema` (not Newtonsoft).
- Stateless: no static mutable state; the SDK supplies `Context` per
  invocation.

## Forbidden

1. `[Fact(Skip=...)]` / silent `try/catch` to mask a failing assertion.
2. New assertion without a sibling test class.
3. Newtonsoft.Json for schema work — drafts come from `Json.Schema`.
4. Reaching into Runner / Mocker internals.
5. Mutating input data (`Data<object>`) — read-only contract.

## Build

```bash
dotnet build ../QaaS.Common.Assertions.sln --nologo -clp:ErrorsOnly
csharpier format <changed-files>
```

Framework SDK alignment: pinned in the repo `Directory.Build.props`.
