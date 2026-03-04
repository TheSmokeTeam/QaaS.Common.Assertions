using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithValidJsonBytes_ShouldReturnTrue()
    {
        const string outputName = "test";
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = outputName,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = System.Text.Encoding.UTF8.GetBytes("{\"id\":1}") }
                    }
                }
            }
        };
        var assertion = new QaaS.Common.Assertions.DeserializationLogic.OutputDeserializableTo
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new OutputDeserializableToConfiguration
            {
                OutputName = outputName,
                Deserialize = new DeserializeConfig
                {
                    Deserializer = SerializationType.Json
                }
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.True);
    }

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithMixedValidAndInvalidJsonBytes_ShouldReturnFalse()
    {
        const string outputName = "test";
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = outputName,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = System.Text.Encoding.UTF8.GetBytes("{\"id\":1}") },
                        new() { Body = System.Text.Encoding.UTF8.GetBytes("not-json") }
                    }
                }
            }
        };
        var assertion = new QaaS.Common.Assertions.DeserializationLogic.OutputDeserializableTo
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new OutputDeserializableToConfiguration
            {
                OutputName = outputName,
                Deserialize = new DeserializeConfig
                {
                    Deserializer = SerializationType.Json
                }
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.False);
        StringAssert.Contains("could not be deserialized", assertion.AssertionMessage!);
        StringAssert.Contains("index 1", assertion.AssertionTrace!);
    }
}
