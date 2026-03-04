using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Common.Assertions.Hermetic;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.HermeticTests;

[TestFixture]
public class HermeticByInputOutputPercentageTests
{
    [Test, 
     TestCase(0, 0),
     TestCase(0, 6),
     TestCase(6, 0),
     TestCase(6, 6),
     TestCase(6, 1000),
     TestCase(1000, 6),
     TestCase(1000, 1000)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithCorrectExpectedPercentage_ShouldReturnTrue(
        int numberOfOutputsDivisibleBy2, int numberOfOutputSourcesDivisibleBy2)
    {
        // Arrange
        var outputNames = new List<string>(numberOfOutputSourcesDivisibleBy2);
        var outputSources = new List<CommunicationData<object>>(numberOfOutputSourcesDivisibleBy2);
        for(var outputSourceNumber = 0; outputSourceNumber < numberOfOutputSourcesDivisibleBy2; outputSourceNumber ++)
        {
            var outputs = new List<DetailedData<object>>();
            for(var outputNumber = 0; outputNumber < numberOfOutputsDivisibleBy2; outputNumber ++)
            {
                outputs.Add(new DetailedData<object>());
            }

            var outputName = outputSourceNumber.ToString();
            outputSources.Add(new CommunicationData<object>
            {
                Name = outputName,
                Data = outputs
            });
            outputNames.Add(outputName);
        }
        
        var inputNames = new List<string>(numberOfOutputSourcesDivisibleBy2/2);
        var inputSources = new List<CommunicationData<object>>(numberOfOutputSourcesDivisibleBy2/2);
        for(var inputSourceNumber = 0; inputSourceNumber < numberOfOutputSourcesDivisibleBy2/2; inputSourceNumber ++)
        {
            var inputs = new List<DetailedData<object>>();
            for(var inputNumber = 0; inputNumber < numberOfOutputsDivisibleBy2/2; inputNumber ++)
            {
                inputs.Add(new DetailedData<object>());
            }

            var inputName = inputSourceNumber.ToString();
            inputSources.Add(new CommunicationData<object>
            {
                Name = inputName,
                Data = inputs
            });
            inputNames.Add(inputName);
        }
        
        var configurations = new HermeticByInputOutputPercentageConfiguration
        {
            OutputNames = outputNames.ToArray(),
            InputNames = inputNames.ToArray(),
            ExpectedPercentage = 400
        };
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = outputSources,
            Inputs = inputSources
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var assertion = new HermeticByInputOutputPercentage
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.IsTrue(result);
    }

    [Test,
     TestCase(6, 6),
     TestCase(6, 1000),
     TestCase(1000, 6),
     TestCase(1000, 1000)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithIncorrectExpectedPercentage_ShouldReturnFalse(
        int numberOfOutputsDivisibleBy2, int numberOfOutputSourcesDivisibleBy2)
    {
        // Arrange
        var outputNames = new List<string>(numberOfOutputSourcesDivisibleBy2);
        var outputSources = new List<CommunicationData<object>>(numberOfOutputSourcesDivisibleBy2);
        for(var outputSourceNumber = 0; outputSourceNumber < numberOfOutputSourcesDivisibleBy2; outputSourceNumber ++)
        {
            var outputs = new List<DetailedData<object>>();
            for(var outputNumber = 0; outputNumber < numberOfOutputsDivisibleBy2; outputNumber ++)
            {
                outputs.Add(new DetailedData<object>());
            }

            var outputName = outputSourceNumber.ToString();
            outputSources.Add(new CommunicationData<object>
            {
                Name = outputName,
                Data = outputs
            });
            outputNames.Add(outputName);
        }
        
        var inputNames = new List<string>(numberOfOutputSourcesDivisibleBy2/2);
        var inputSources = new List<CommunicationData<object>>(numberOfOutputSourcesDivisibleBy2/2);
        for(var inputSourceNumber = 0; inputSourceNumber < numberOfOutputSourcesDivisibleBy2/2; inputSourceNumber ++)
        {
            var inputs = new List<DetailedData<object>>();
            for(var inputNumber = 0; inputNumber < numberOfOutputsDivisibleBy2/2; inputNumber ++)
            {
                inputs.Add(new DetailedData<object>());
            }

            var inputName = inputSourceNumber.ToString();
            inputSources.Add(new CommunicationData<object>
            {
                Name = inputName,
                Data = inputs
            });
            inputNames.Add(inputName);
        }
        
        var configurations = new HermeticByInputOutputPercentageConfiguration
        {
            OutputNames = outputNames.ToArray(),
            InputNames = inputNames.ToArray(),
            ExpectedPercentage = 50
        };
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = outputSources,
            Inputs = inputSources
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var assertion = new HermeticByInputOutputPercentage
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithInputsAreOutputs_ShouldUseOutputsAsInputSource()
    {
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = "inputAsOutput", Data = new List<DetailedData<object>> { new(), new(), new(), new() } },
                new() { Name = "result", Data = new List<DetailedData<object>> { new(), new() } }
            }
        };
        var assertion = new HermeticByInputOutputPercentage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new HermeticByInputOutputPercentageConfiguration
            {
                InputNames = new[] { "inputAsOutput" },
                OutputNames = new[] { "result" },
                InputsAreOutputs = true,
                ExpectedPercentage = 50
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.True);
    }

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithNoInputsAndNoOutputs_ShouldReturnTrueWithoutDivisionError()
    {
        var session = new SessionData
        {
            Name = "Id1",
            Inputs = new List<CommunicationData<object>>(),
            Outputs = new List<CommunicationData<object>>()
        };
        var assertion = new HermeticByInputOutputPercentage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new HermeticByInputOutputPercentageConfiguration
            {
                InputNames = new[] { "missing-input" },
                OutputNames = new[] { "missing-output" },
                ExpectedPercentage = 400
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.True);
        StringAssert.Contains("percentage between the total output count and total input count is 0", assertion.AssertionMessage!);
    }

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithNoInputsButWithOutputs_ShouldReturnFalseWithoutDivisionError()
    {
        var session = new SessionData
        {
            Name = "Id1",
            Inputs = new List<CommunicationData<object>>(),
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = "out", Data = new List<DetailedData<object>> { new() } }
            }
        };
        var assertion = new HermeticByInputOutputPercentage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new HermeticByInputOutputPercentageConfiguration
            {
                InputNames = new[] { "missing-input" },
                OutputNames = new[] { "out" },
                ExpectedPercentage = 100
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.False);
    }
}
