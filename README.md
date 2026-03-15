# QaaS.Common.Assertions

Composable .NET package for validating QaaS test workflows.

[![CI](https://github.com/TheSmokeTeam/QaaS.Common.Assertions/actions/workflows/ci.yml/badge.svg)](https://github.com/TheSmokeTeam/QaaS.Common.Assertions/actions/workflows/ci.yml)
[![Line Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/eldarush/509510e0964a67e6182297b28857c07b/raw/line-coverage-badge.json)](https://github.com/TheSmokeTeam/QaaS.Common.Assertions/actions/workflows/ci.yml)
[![Branch Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/eldarush/509510e0964a67e6182297b28857c07b/raw/branch-coverage-badge.json)](https://github.com/TheSmokeTeam/QaaS.Common.Assertions/actions/workflows/ci.yml)
[![Docs](https://img.shields.io/badge/docs-qaas--docs-blue)](https://thesmoketeam.github.io/qaas-docs/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

## Contents
- [Overview](#overview)
- [Packages](#packages)
- [Functionalities](#functionalities)
- [Protocol Support](#protocol-support)
- [Quick Start](#quick-start)
- [Build and Test](#build-and-test)
- [Documentation](#documentation)

## Overview
This repository contains one solution: [`QaaS.Common.Assertions.sln`](./QaaS.Common.Assertions.sln).

The solution is split into a focused package project and a test project so you can consume assertions as a NuGet package and validate behavior with a dedicated NUnit suite.

## Packages
| Package | Latest Version | Total Downloads |
|---|---|---|
| [QaaS.Common.Assertions](https://www.nuget.org/packages?q=QaaS.Common.Assertions) | [![NuGet](https://img.shields.io/nuget/v/QaaS.Common.Assertions?logo=nuget)](https://www.nuget.org/packages?q=QaaS.Common.Assertions) | [![Downloads](https://img.shields.io/nuget/dt/QaaS.Common.Assertions?logo=nuget)](https://www.nuget.org/packages?q=QaaS.Common.Assertions) |

## Functionalities
### [QaaS.Common.Assertions](./QaaS.Common.Assertions/)
- `HermeticByExpectedOutputCount`: validates output totals against an exact expected count.
- `HermeticByExpectedOutputCountInRange`: validates output totals inside min/max bounds.
- `HermeticByInputOutputPercentage`: validates output count as an expected percentage of input count.
- `HermeticByInputOutputPercentageInRange`: validates output/input percentage inside a range.
- `ValidateHermeticMetricsByInputOutputPercentage`: compares metrics-based hermetic percentage to IO-based percentage with tolerance.
- `DelayByAverage`: validates average output delay against average input timestamp.
- `DelayByChunks`: validates per-chunk delay with configurable chunk size and chunk timestamp strategy.
- `OutputContentByExpectedCsvResults`: validates output payload fields against expected CSV results.
- Field-level validators include exact value, error range, exact override value, and base64-to-hex matching.
- Supports JSON conversion from `Json`, `Xml`, and `Object` inputs.
- `ObjectOutputJsonSchema`: validates each output item against one or more JSON schemas provided by data sources.
- Supports multiple JSON schema drafts (06/07/2019-09/2020-12/next).
- `OutputDeserializableTo`: validates that all output items are deserializable with the configured QaaS deserializer.
- `HttpStatus`: validates that all configured output items contain the expected HTTP status code.

### [QaaS.Common.Assertions.Tests](./QaaS.Common.Assertions.Tests/)
- NUnit-based verification of assertion behaviors, edge cases, and failure traces.
- Covers hermetic assertions, delay assertions, content validation, schema validation, deserialization, and HTTP metadata assertions.

## Protocol Support
Supported assertion families in `QaaS.Common.Assertions`:

| Family | Implementations |
|---|---|
| Hermetic | `HermeticByExpectedOutputCount`, `HermeticByExpectedOutputCountInRange`, `HermeticByInputOutputPercentage`, `HermeticByInputOutputPercentageInRange`, `ValidateHermeticMetricsByInputOutputPercentage` |
| Delay | `DelayByAverage`, `DelayByChunks` |
| Content Validation | `OutputContentByExpectedCsvResults` with exact/error-range/override/base64-to-hex field validators |
| Schema Validation | `ObjectOutputJsonSchema` |
| Deserialization | `OutputDeserializableTo` |
| HTTP Metadata | `HttpStatus` |

## Quick Start
Install package:

```bash
dotnet add package QaaS.Common.Assertions
```

Update package:

```bash
dotnet add package QaaS.Common.Assertions --version <TARGET_VERSION>
dotnet restore
```

## Build and Test
```bash
dotnet restore QaaS.Common.Assertions.sln
dotnet build QaaS.Common.Assertions.sln -c Release --no-restore
dotnet test QaaS.Common.Assertions.sln -c Release --no-build
```

## Documentation
- Official docs: [thesmoketeam.github.io/qaas-docs](https://thesmoketeam.github.io/qaas-docs/)
- CI workflow: [`.github/workflows/ci.yml`](./.github/workflows/ci.yml)
- NuGet feed query: [QaaS.Common.Assertions on NuGet search](https://www.nuget.org/packages?q=QaaS.Common.Assertions)
