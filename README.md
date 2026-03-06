# QaaS.Common.Assertions

Composable assertion package for validating QaaS test workflows.

[![CI](https://img.shields.io/badge/CI-GitHub_Actions-2088FF)](./.github/workflows/ci.yml)
[![Docs](https://img.shields.io/badge/docs-qaas--docs-blue)](https://thesmoketeam.github.io/qaas-docs/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

## Contents
- [Overview](#overview)
- [Packages](#packages)
- [Functionalities](#functionalities)
- [Quick Start](#quick-start)
- [Build and Test](#build-and-test)
- [Documentation](#documentation)

## Overview
This repository contains one solution: [`QaaS.Common.Assertions.sln`](./QaaS.Common.Assertions.sln).

The solution is split into:
- [`QaaS.Common.Assertions`](./QaaS.Common.Assertions/) - the NuGet package with built-in assertion implementations.
- [`QaaS.Common.Assertions.Tests`](./QaaS.Common.Assertions.Tests/) - NUnit tests validating assertion behavior and edge cases.

## Packages
| Package | Latest Version | Total Downloads |
|---|---|---|
| [QaaS.Common.Assertions](https://www.nuget.org/packages?q=QaaS.Common.Assertions) | [![NuGet](https://img.shields.io/nuget/v/QaaS.Common.Assertions?logo=nuget)](https://www.nuget.org/packages?q=QaaS.Common.Assertions) | [![Downloads](https://img.shields.io/nuget/dt/QaaS.Common.Assertions?logo=nuget)](https://www.nuget.org/packages?q=QaaS.Common.Assertions) |

## Functionalities
### [Hermetic Assertions](./QaaS.Common.Assertions/Hermetic/)
- `HermeticByExpectedOutputCount`: validates output totals against an exact expected count.
- `HermeticByExpectedOutputCountInRange`: validates output totals inside min/max bounds.
- `HermeticByInputOutputPercentage`: validates output count as an expected percentage of input count.
- `HermeticByInputOutputPercentageInRange`: validates output/input percentage inside a range.
- `ValidateHermeticMetricsByInputOutputPercentage`: compares metrics-based hermetic percentage to IO-based percentage with tolerance.

### [Delay Assertions](./QaaS.Common.Assertions/Delay/)
- `DelayByAverage`: validates average output delay against average input timestamp.
- `DelayByChunks`: validates per-chunk delay with configurable chunk size and chunk timestamp strategy.
- Supports negative-delay buffer configuration for clock/order tolerance.

### [Content Assertions](./QaaS.Common.Assertions/ContentLogic/)
- `OutputContentByExpectedCsvResults`: validates output payload fields against expected CSV results.
- Field-level validators include exact value, error range, exact override value, and base64-to-hex matching.
- Supports JSON conversion from `Json`, `Xml`, and `Object` inputs.

### [Schema Assertions](./QaaS.Common.Assertions/SchemaLogic/)
- `ObjectOutputJsonSchema`: validates each output item against one or more JSON schemas provided by data sources.
- Supports multiple JSON schema drafts (06/07/2019-09/2020-12/next).

### [Deserialization Assertions](./QaaS.Common.Assertions/DeserializationLogic/)
- `OutputDeserializableTo`: validates that all output items are deserializable with the configured QaaS deserializer.

### [HTTP Metadata Assertions](./QaaS.Common.Assertions/HttpMetaDataLogic/)
- `HttpStatus`: validates that all configured output items contain the expected HTTP status code.

## Quick Start
Install package:

```bash
dotnet add package QaaS.Common.Assertions
```

Assertion example (`DelayByAverage`):

```csharp
using System.Collections.Immutable;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;
using QaaS.Common.Assertions.Delay;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

var assertion = new DelayByAverage
{
    Configuration = new DelayByAverageConfiguration
    {
        InputName = "input-topic",
        OutputName = "output-topic",
        MaximumDelayMs = 500
    }
};

bool passed = assertion.Assert(
    sessionDataList: ImmutableList<SessionData>.Empty,
    dataSourceList: ImmutableList<DataSource>.Empty
);
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
