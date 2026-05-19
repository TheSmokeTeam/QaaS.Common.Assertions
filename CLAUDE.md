# CLAUDE.md — QaaS.Common.Assertions

## Purpose
Reusable assertion hooks for QaaS test workflows. Implements `IAssertion` from `QaaS.Framework.SDK`.

## Assertion Types
- **Hermetic**: `HermeticByExpectedOutputCount`, `HermeticByExpectedOutputCountInRange`, `HermeticByInputOutputPercentage`, `HermeticByInputOutputPercentageInRange`, `ValidateHermeticMetricsByInputOutputPercentage`
- **Delay**: `DelayByAverage`, `DelayByChunks`
- **Content**: `OutputContentByExpectedCsvResults` (exact/error-range/override/base64-hex field validators)
- **Schema**: `ObjectOutputJsonSchema` (JSON Schema drafts 06/07/2019-09/2020-12)
- **Deserialization**: `OutputDeserializableTo`
- **HTTP**: `HttpStatus`

## Build & Test
```bash
dotnet build QaaS.Common.Assertions.sln
dotnet test QaaS.Common.Assertions.sln
```
