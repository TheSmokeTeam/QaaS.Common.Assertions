using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Common.Assertions.Hermetic;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.HermeticTests;

[TestFixture]
public class ValidateHermeticMetricsByInputOutputPercentageTests
{
    private const string NameProperty = "__name__",
        ValueProperty = "value",
        Metric = "metric",
        InputMetricName = "input",
        OutputMetricName = "output",
        SessionName = "session",
        CollectorName = "outputMetrics";

    [Test,
     TestCase(1, 1, 1, 1, true),
     TestCase(1, 10, 1, 10, true),
     TestCase(10, 10, 10, 10, true),
     TestCase(50, 10, 50, 10, true),
     TestCase(50, 50, 50, 50, true),
     TestCase(50, 50, 50, 1, false),
     TestCase(10, 10, 1, 10, false)]
    public void
        TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithConfiguredMetricsAndInputOutputValues_AssertionShouldReturnExpectedResult(
            int inputCount, int outputCount, int inputMetricValue, int outputMetricValue, bool expectedResult)
    {
        // Arrange
        var inputData = Enumerable.Range(0, inputCount).Select(input => new DetailedData<object>()
        {
            Body = input,
            Timestamp = DateTime.UtcNow
        }).ToList();
        var outputData = Enumerable.Range(0, outputCount).Select(output => new DetailedData<object>()
        {
            Body = output,
            Timestamp = DateTime.UtcNow
        }).ToList();
        var outputMetricResult = Enumerable.Range(0, outputCount).Select(_ => new DetailedData<object>()
        {
            Body = new JsonObject
            {
                {
                    Metric, new JsonObject
                    {
                        { NameProperty, OutputMetricName },
                    }
                },
                { ValueProperty, outputMetricValue.ToString() }
            },
            Timestamp = DateTime.UtcNow
        }).ToList();
        var inputMetricResult = Enumerable.Range(0, inputCount).Select(_ => new DetailedData<object>()
        {
            Body = new JsonObject
            {
                {
                    Metric, new JsonObject
                    {
                        { NameProperty, InputMetricName },
                    }
                },
                { ValueProperty, inputMetricValue.ToString() }
            },
            Timestamp = DateTime.UtcNow
        }).ToList();
        var input = new CommunicationData<object>()
        {
            Name = "input",
            Data = inputData
        };
        var output = new CommunicationData<object>()
        {
            Name = "output",
            Data = outputData
        };
        var outputMetrics = new CommunicationData<object>()
        {
            Name = CollectorName,
            Data = inputMetricResult.Concat(outputMetricResult).ToList()
        };
        var sessionDataList = new List<SessionData>()
        {
            new()
            {
                Name = SessionName,
                Outputs = [output, outputMetrics],
                Inputs = [input]
            }
        }.ToImmutableList();
        var assertion = new ValidateHermeticMetricsByInputOutputPercentage()
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new ValidateHermeticMetricsByInputOutputPercentageConfig()
            {
                InputMetricName = InputMetricName,
                OutputMetricName = OutputMetricName,
                InputNames = ["input"],
                OutputNames = ["output"],
                MetricOutputSourceName = CollectorName
            }
        };

        // Act 
        var result = assertion.Assert(sessionDataList, new ImmutableArray<DataSource>());

        // Assert
        Assert.True(result == expectedResult);
    }

    [Test]
    public void
        TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithInvalidMetrics_AssertionShouldThrowError()
    {
        // Arrange
        var inputData = Enumerable.Range(0, 10).Select(input => new DetailedData<object>()
        {
            Body = input,
            Timestamp = DateTime.UtcNow
        }).ToList();
        var outputData = Enumerable.Range(0, 10).Select(output => new DetailedData<object>()
        {
            Body = output,
            Timestamp = DateTime.UtcNow
        }).ToList();
        var outputMetricResult = Enumerable.Range(0, 10).Select(_ => new DetailedData<object>()
        {
            Body = new JsonObject
            {
                {
                    Metric, new JsonObject
                    {
                        { NameProperty, OutputMetricName },
                    }
                },
            },
            Timestamp = DateTime.UtcNow
        }).ToList();

        var input = new CommunicationData<object>()
        {
            Name = "input",
            Data = inputData
        };
        var output = new CommunicationData<object>()
        {
            Name = "output",
            Data = outputData
        };
        var outputMetrics = new CommunicationData<object>()
        {
            Name = CollectorName,
            Data = outputMetricResult
        };
        var sessionDataList = new List<SessionData>()
        {
            new()
            {
                Name = SessionName,
                Outputs = [output, outputMetrics],
                Inputs = [input]
            }
        }.ToImmutableList();
        var assertion = new ValidateHermeticMetricsByInputOutputPercentage()
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new ValidateHermeticMetricsByInputOutputPercentageConfig()
            {
                InputMetricName = InputMetricName,
                OutputMetricName = OutputMetricName,
                InputNames = ["input"],
                OutputNames = ["output"],
                MetricOutputSourceName = CollectorName
            }
        };


        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            assertion.Assert(sessionDataList, new ImmutableArray<DataSource>()));
    }
}