using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using QaaS.Common.Assertions.CommonAssertionsConfigs.HttpMetaDataLogic;
using QaaS.Common.Assertions.HttpMetaDataLogic;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.HttpMetaDataLogicTests;

[TestFixture]
public class HttpStatusTests
{
    [Test, 
     TestCase(0, 0, 210),
     TestCase(0, 1, 200),
     TestCase(1, 0, 100),
     TestCase(1, 1, 500),
     TestCase(1, 1000, 101),
     TestCase(1000, 1, 100),
     TestCase(1000, 1000, 599)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithCorrectStatusCodes_ShouldReturnTrue(
        int numberOfOutputs, int numberOfOutputSources, int statusCode)
    {
        // Arrange
        var outputNames = new List<string>(numberOfOutputSources);
        var outputSources = new List<CommunicationData<object>>(numberOfOutputSources);
        for(var outputSourceNumber = 0; outputSourceNumber < numberOfOutputSources; outputSourceNumber ++)
        {
            var outputs = new List<DetailedData<object>>();
            for(var outputNumber = 0; outputNumber < numberOfOutputs; outputNumber ++)
            {
                outputs.Add(new DetailedData<object>{MetaData = new MetaData{Http = new Http{StatusCode = statusCode}}});
            }

            var outputName = outputSourceNumber.ToString();
            outputSources.Add(new CommunicationData<object>
            {
                Name = outputName,
                Data = outputs
            });
            outputNames.Add(outputName);
        }
        
        var configurations = new HttpStatusConfiguration
        {
            OutputNames = outputNames.ToArray(),
            StatusCode = statusCode
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
        var assertion = new HttpStatus
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
     TestCase(1, 1, 500),
     TestCase(1, 1000, 101),
     TestCase(1000, 1, 100),
     TestCase(1000, 1000, 599)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithIncorrectStatusCodes_ShouldReturnFalse(
        int numberOfOutputs, int numberOfOutputSources, int statusCode)
    {
        // Arrange
        var incorrectStatusCode = statusCode + 1;
        var outputNames = new List<string>(numberOfOutputSources);
        var outputSources = new List<CommunicationData<object>>(numberOfOutputSources);
        for(var outputSourceNumber = 0; outputSourceNumber < numberOfOutputSources; outputSourceNumber ++)
        {
            var outputs = new List<DetailedData<object>>();
            for(var outputNumber = 0; outputNumber < numberOfOutputs; outputNumber ++)
            {
                outputs.Add(new DetailedData<object>{MetaData = new MetaData{Http = new Http{StatusCode = incorrectStatusCode}}});
            }

            var outputName = outputSourceNumber.ToString();
            outputSources.Add(new CommunicationData<object>
            {
                Name = outputName,
                Data = outputs
            });
            outputNames.Add(outputName);
        }
        
        var configurations = new HttpStatusConfiguration
        {
            OutputNames = outputNames.ToArray(),
            StatusCode = statusCode
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
        var assertion = new HttpStatus
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());
        Globals.Logger.LogInformation($"AssertionFailureMessage: {assertion.AssertionMessage}");
        
        // Assert
        Assert.IsFalse(result);
        StringAssert.Contains((numberOfOutputs*numberOfOutputSources).ToString(), assertion.AssertionMessage!);
        StringAssert.Contains(incorrectStatusCode.ToString(), assertion.AssertionTrace!);
    }
    
    [Test,
     TestCase(1, 1, 500),
     TestCase(1, 1000, 101),
     TestCase(1000, 1, 100),
     TestCase(1000, 1000, 599)]
    public void TestAssertSingleSession_CallAssertFunctionWithASingleSessionWithNoStatusCodes_ShouldThrowArgumentException(
        int numberOfOutputs, int numberOfOutputSources, int statusCode)
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
        
        var configurations = new HttpStatusConfiguration
        {
            OutputNames = outputNames.ToArray(),
            StatusCode = statusCode
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
        var assertion = new HttpStatus
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act + Assert
        Assert.Throws<ArgumentException>(() =>
            assertion.Assert(sessionList, new ImmutableArray<DataSource>()));
    }

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithMixedNoStatusAndIncorrectStatus_ShouldThrowForMissingStatus()
    {
        var outputName = "output";
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
                        new() { MetaData = new MetaData { Http = new Http { StatusCode = 500 } } },
                        new()
                    }
                }
            }
        };
        var assertion = new HttpStatus
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new HttpStatusConfiguration
            {
                OutputNames = new[] { outputName },
                StatusCode = 200
            }
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty));

        StringAssert.Contains("have no Http Status", exception!.Message);
    }

    [Test]
    public void TestAssertSingleSession_CallAssertFunctionWithNoItems_ShouldReturnTrue()
    {
        var outputName = "output";
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = outputName,
                    Data = new List<DetailedData<object>>()
                }
            }
        };
        var assertion = new HttpStatus
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new HttpStatusConfiguration
            {
                OutputNames = new[] { outputName },
                StatusCode = 200
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.True);
        StringAssert.Contains("arrived with status 200", assertion.AssertionMessage!);
    }
}
