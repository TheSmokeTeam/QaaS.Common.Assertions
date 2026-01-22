using System.Collections.Generic;
using System.Collections.Immutable;
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
public class HermeticByInputOutputPercentageInRangeTests
{
    [Test,
     TestCase(6, 6),
     TestCase(6, 1000),
     TestCase(1000, 6),
     TestCase(1000, 1000)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithPercentageInLimits_ShouldReturnTrue(
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
        
        var configurations = new HermeticByInputOutputPercentageInRangeConfiguration()
        {
            OutputNames = outputNames.ToArray(),
            InputNames = inputNames.ToArray(),
            ExpectedMaximumPercentage = 600,
            ExpectedMinimumPercentage = 200
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
        var assertion = new HermeticByInputOutputPercentageInRange
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
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithPercentageNotInLimits_ShouldReturnFalse(
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
        
        var configurations = new HermeticByInputOutputPercentageInRangeConfiguration
        {
            OutputNames = outputNames.ToArray(),
            InputNames = inputNames.ToArray(),
            ExpectedMaximumPercentage = 300,
            ExpectedMinimumPercentage = 100
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
        var assertion = new HermeticByInputOutputPercentageInRange
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.IsFalse(result);
    }

}