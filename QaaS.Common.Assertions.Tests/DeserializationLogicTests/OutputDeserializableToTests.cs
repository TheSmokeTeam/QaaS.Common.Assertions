using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.DeserializationLogic;
using QaaS.Common.Assertions.Tests.Mocks;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;
using QaaS.Framework.Serialization;

namespace QaaS.Common.Assertions.Tests.DeserializationLogicTests;

[TestFixture]
public class OutputDeserializableToTests
{
    [Test,
     TestCase(0, true),
     TestCase(1, false),
     TestCase(10, false)]
    public void
        TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithDeserializableItems_ShouldReturnExpectedAssertionResult
        (int numberOfOutputs, bool shouldPassAssertion)
    {
        // Arrange
        const string outputName = "test";

        var outputs = new List<DetailedData<object>>();
        for (var outputNumber = 0; outputNumber < numberOfOutputs; outputNumber++)
        {
            outputs.Add(new DetailedData<object> { Body = Array.Empty<byte>() });
        }

        var outputSource = new CommunicationData<object>
        {
            Name = outputName,
            Data = outputs
        };
        var configurations = new OutputDeserializableToConfiguration
        {
            OutputName = outputName,
            Deserialize = new DeserializeConfig
            {
                Deserializer = SerializationType.Json,
                SpecificType = null
            }
        };
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = new List<CommunicationData<object>> { outputSource }
        };

        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();


        var assertion = new OutputDeserializableToMock(shouldPassAssertion
            ? new MockDeserializerWorksWithAllData()
            : new MockDeserializerWorksWithNoData())
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations
        };

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.AreEqual(shouldPassAssertion, result);
    }
}