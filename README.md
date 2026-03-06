# QaaS.Common.Assertions

[![NuGet Version](https://img.shields.io/nuget/v/QaaS.Common.Assertions?logo=nuget)](https://www.nuget.org/packages?q=QaaS.Common.Assertions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/QaaS.Common.Assertions?logo=nuget)](https://www.nuget.org/packages?q=QaaS.Common.Assertions)
[![CI](https://img.shields.io/github/actions/workflow/status/TheSmokeTeam/QaaS.Common.Assertions/ci.yml?branch=master&label=CI)](.github/workflows/ci.yml)
[![Line Coverage](https://img.shields.io/badge/line%20coverage-96.07%25-brightgreen)](.github/workflows/ci.yml)
[![Branch Coverage](https://img.shields.io/badge/branch%20coverage-82.12%25-yellowgreen)](.github/workflows/ci.yml)
[![QaaS Docs](https://img.shields.io/badge/docs-qaas--docs-blue)](https://thesmoketeam.github.io/qaas-docs/)

Common assertion package for QaaS pipelines.  
This solution-level README documents `QaaS.Common.Assertions.sln`.

## Contents

- [What This Solution Contains](#what-this-solution-contains)
- [NuGet Package](#nuget-package)
- [Assertion Catalog](#assertion-catalog)
- [Quick Start](#quick-start)
- [Configuration Highlights](#configuration-highlights)
- [Test Coverage](#test-coverage)
- [Documentation](#documentation)

## What This Solution Contains

| Project | Type | Purpose |
| --- | --- | --- |
| `QaaS.Common.Assertions` | Package (`net10.0`) | Assertion implementations and configuration models for hermeticity, delay, content validation, schema validation, deserialization checks, and HTTP metadata validation. |
| `QaaS.Common.Assertions.Tests` | NUnit tests (`net10.0`) | Automated tests that validate all assertion behaviors and edge cases. |

## NuGet Package

Install:

```bash
dotnet add package QaaS.Common.Assertions
```

Package links:
- [QaaS.Common.Assertions search on NuGet](https://www.nuget.org/packages?q=QaaS.Common.Assertions)

## Assertion Catalog

| Area | Assertion Class | Configuration Class | DataSources | Session Support |
| --- | --- | --- | --- | --- |
| Hermetic | `HermeticByExpectedOutputCount` | `HermeticByExpectedOutputCountConfiguration` | Not used | Multi-session |
| Hermetic | `HermeticByExpectedOutputCountInRange` | `HermeticByExpectedOutputCountInRangeConfiguration` | Not used | Multi-session |
| Hermetic | `HermeticByInputOutputPercentage` | `HermeticByInputOutputPercentageConfiguration` | Not used | Multi-session |
| Hermetic | `HermeticByInputOutputPercentageInRange` | `HermeticByInputOutputPercentageInRangeConfiguration` | Not used | Multi-session |
| Hermetic | `ValidateHermeticMetricsByInputOutputPercentage` | `ValidateHermeticMetricsByInputOutputPercentageConfig` | Not used | Multi-session |
| Delay | `DelayByAverage` | `DelayByAverageConfiguration` | Not used | Single-session |
| Delay | `DelayByChunks` | `DelayByChunksConfiguration` | Not used | Single-session |
| Content | `OutputContentByExpectedCsvResults` | `OutputContentByExpectedResultsAsCsvConfiguration` | Required | Single-session |
| Schema | `ObjectOutputJsonSchema` | `ObjectOutputJsonSchemaConfiguration` | Required | Single-session |
| Deserialization | `OutputDeserializableTo` | `OutputDeserializableToConfiguration` | Not used | Single-session |
| HTTP Metadata | `HttpStatus` | `HttpStatusConfiguration` | Not used | Single-session |

## Quick Start

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

CSV-content assertion example:

```csharp
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Common.Assertions.ContentLogic;

var csvAssertion = new OutputContentByExpectedCsvResults
{
    Configuration = new OutputContentByExpectedResultsAsCsvConfiguration
    {
        OutputName = "output-topic",
        DataSourceName = "expected-results",
        ResultsMetaDataStorageKey = "results.csv",
        ColumnNameToFieldPathMap = new Dictionary<string, FieldConfiguration>
        {
            ["ID"] = new()
            {
                Path = "$.id",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue
                }
            },
            ["LATENCY_MS"] = new()
            {
                Path = "$.metrics.latency",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ErrorRange,
                    ErrorRange = new ErrorRangeFieldValidatorConfig { ErrorRange = 10 }
                }
            }
        }
    }
};
```

## Configuration Highlights

- `DelayByAverageConfiguration`: compares average input and output timestamps with `MaximumDelayMs`.
- `DelayByChunksConfiguration`: compares chunk-to-chunk delays with configurable chunk size and chunk timestamp strategy.
- `HermeticBy*` configurations: validate output counts and percentages against input/output expectations.
- `ValidateHermeticMetricsByInputOutputPercentageConfig`: compares metric-derived hermetic percentage against IO-derived percentage with tolerance.
- `OutputContentByExpectedResultsAsCsvConfiguration`: validates output JSON fields against CSV expected rows using field-level validators.
- `ObjectOutputJsonSchemaConfiguration`: validates each output item against one or more JSON schemas from data sources.
- `OutputDeserializableToConfiguration`: validates all output items can be deserialized by a configured QaaS deserializer.
- `HttpStatusConfiguration`: validates all configured outputs share the expected HTTP status code.

## Test Coverage

Coverage measured from local run on **2026-03-06** using:

```bash
dotnet test QaaS.Common.Assertions.sln --configuration Release --collect:"XPlat Code Coverage"
dotnet test QaaS.Common.Assertions.sln --configuration Release --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.IncludeTestAssembly=true
```

| Project | Coverage |
| --- | --- |
| `QaaS.Common.Assertions` | ![QaaS.Common.Assertions Coverage](https://img.shields.io/badge/coverage-96.07%25-brightgreen) Line, ![QaaS.Common.Assertions Branch](https://img.shields.io/badge/branch-82.12%25-yellowgreen) Branch |
| `QaaS.Common.Assertions.Tests` | ![QaaS.Common.Assertions.Tests Coverage](https://img.shields.io/badge/coverage-97.57%25-brightgreen) Line, ![QaaS.Common.Assertions.Tests Branch](https://img.shields.io/badge/branch-74.13%25-yellowgreen) Branch |

Test execution summary:
- `207` passed
- `0` failed
- `0` skipped

## Documentation

- [QaaS Documentation Portal](https://thesmoketeam.github.io/qaas-docs/)
- [Repository Root](.)
- [CI Workflow](.github/workflows/ci.yml)
