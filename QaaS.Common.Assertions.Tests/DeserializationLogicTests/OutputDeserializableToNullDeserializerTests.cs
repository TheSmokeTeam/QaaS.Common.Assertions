using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.DeserializationLogic;
using QaaS.Common.Assertions.DeserializationLogic;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;
using QaaS.Framework.Serialization;

namespace QaaS.Common.Assertions.Tests.DeserializationLogicTests;

/// <summary>
/// Tests for C-1: null deserializer factory result must be treated as a hard config error.
/// </summary>
[TestFixture]
public class OutputDeserializableToNullDeserializerTests
{
    /// <summary>
    /// Subclass that allows injecting a custom DeserializerFactory result via a field swap.
    /// We use reflection to call DeserializerFactory with a null type to produce a null deserializer.
    /// </summary>
    private sealed class NullDeserializerAssertion : OutputDeserializableTo
    {
        // Expose ability to test by forcing a null-returning configuration:
        // Configuration.Deserialize.Deserializer is left null so BuildDeserializer(null) -> null.
    }

    [Test]
    public void Assert_WhenDeserializerFactoryReturnsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange — BuildDeserializer(null) returns null (framework contract documented in DeserializerFactory).
        // We reach this path by passing a null Deserializer type to the config;
        // the assertion should throw before even iterating output.
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
                    },
                },
            },
        };

        var assertion = new NullDeserializerAssertion
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new OutputDeserializableToConfiguration
            {
                OutputName = outputName,
                // Deserializer is null -> DeserializerFactory.BuildDeserializer(null) -> null
                Deserialize = new DeserializeConfig { Deserializer = null },
            },
        };

        // Act + Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            assertion.Assert(
                new List<SessionData> { session }.ToImmutableList(),
                ImmutableList<DataSource>.Empty
            )
        );

        Assert.That(ex!.Message, Does.Contain("configuration error"));
    }

    [Test]
    public void Assert_WhenDeserializerIsValid_ShouldNotThrow()
    {
        // Sanity check: a valid deserializer type must still work correctly.
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
                    },
                },
            },
        };

        var assertion = new OutputDeserializableTo
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new OutputDeserializableToConfiguration
            {
                OutputName = outputName,
                Deserialize = new DeserializeConfig { Deserializer = SerializationType.Json },
            },
        };

        Assert.DoesNotThrow(() =>
            assertion.Assert(
                new List<SessionData> { session }.ToImmutableList(),
                ImmutableList<DataSource>.Empty
            )
        );
    }
}
