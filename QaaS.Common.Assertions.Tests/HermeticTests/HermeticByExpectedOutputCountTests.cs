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
public class HermeticByExpectedOutputCountTests
{
    [Test, 
     TestCase(0, 0),
     TestCase(0, 1),
     TestCase(1, 0),
     TestCase(1, 1),
     TestCase(1, 1000),
     TestCase(1000, 1),
     TestCase(1000, 1000)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithCorrectExpectedCount_ShouldReturnTrue(
        int numberOfOutputs, int numberOfOutputSources)
    {
        // Arrange
        var outputNames = new List<string>(numberOfOutputSources);
        var outputSources = new List<CommunicationData<object>>(numberOfOutputSources);
        for(var outputSourceNumber = 0; outputSourceNumber < numberOfOutputSources; outputSourceNumber ++)
        {
            var outputs = new List<DetailedData<object>>();
            for(var outputNumber = 0; outputNumber < numberOfOutputs; outputNumber ++)
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
        
        var configurations = new HermeticByExpectedOutputCountConfiguration
        {
            OutputNames = outputNames.ToArray(),
            ExpectedCount = numberOfOutputs*numberOfOutputSources
        };
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = outputSources
        };
        var assertion = new HermeticByExpectedOutputCount
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.IsTrue(result);
    }
    
    [Test,
     TestCase(0, 0),
     TestCase(0, 1),
     TestCase(1, 0),
     TestCase(1, 1),
     TestCase(1, 1000),
     TestCase(1000, 1),
     TestCase(1000, 1000)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithIncorrectExpectedCount_ShouldReturnFalse(
        int numberOfOutputs, int numberOfOutputSources)
    {
        // Arrange
        var outputNames = new List<string>(numberOfOutputSources);
        var outputSources = new List<CommunicationData<object>>(numberOfOutputSources);
        for(var outputSourceNumber = 0; outputSourceNumber < numberOfOutputSources; outputSourceNumber ++)
        {
            var outputs = new List<DetailedData<object>>();
            for(var outputNumber = 0; outputNumber < numberOfOutputs; outputNumber ++)
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
        
        var configurations = new HermeticByExpectedOutputCountConfiguration
        {
            OutputNames = outputNames.ToArray(),
            ExpectedCount = numberOfOutputs*numberOfOutputSources + 2
        };
        var session = new SessionData
        {
            
            Name = "Id1",
            Outputs = outputSources
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var assertion = new HermeticByExpectedOutputCount
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