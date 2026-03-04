using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;
using QaaS.Common.Assertions.Delay;
using QaaS.Common.Assertions.Delay.Exceptions;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.DelayTests;

[TestFixture]
public class DelayByAverageTests
{
    // Tests for AverageArrivedOnTime Function below 
    [Test]
    [TestCase(2000, 3, 0, true, 0)]
    [TestCase(2000, 3, 3, true, 1000)]
    [TestCase(2000, 3, 3, false, 4000)]
    public void
        TestAssertSingleSession_CallFunctionWithASingleSession_ShouldReturnTheSameAsShouldArriveInTimeVariable(
            long maximumDelayMilliSeconds, int inputListSize, int outputListSize, bool shouldArriveInTime,
            int timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds)
    {
        // Arrange
        const string name = "Test";
        var inputList = new List<DetailedData<object>>();
        for (var index = 0; index < inputListSize; index++)
        {
            inputList.Add(new DetailedData<object>{Body=null, Timestamp = 
                new DateTime(index)});
        }
        var input = new CommunicationData<object> { Name = name, Data = inputList };

        var outputList = new List<DetailedData<object>>();
        for (var index = 0; index < outputListSize; index++)
        {
            outputList.Add(new DetailedData<object>{Body=null, Timestamp =
                new DateTime( 
                index + timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds * 10_000)});
        }
        var output = new CommunicationData<object> { Name = name, Data = outputList };

        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                output
            },
            Inputs = new List<CommunicationData<object>>
            {
                input
            }
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
        };
        var assertion = new DelayByAverage
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };

        // Act
        var testResult = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.That(testResult == shouldArriveInTime);
        if (outputListSize > 0)
            StringAssert.Contains($"maximum allowed average delay is {maximumDelayMilliSeconds} milliseconds",
                assertion.AssertionMessage!);
    }
    
    [Test]
    public void
        TestAssertSingleSession_CallFunctionWithWithEmptyInputListAndOutputListInSession_ShouldGetExceptionFromFunction
        ()
    {
        // Arrange
        const long maximumDelayMilliSeconds = 2000;
        const string name = "Test";
        var inputList = new List<DetailedData<object>>();
        var outputList = new List<DetailedData<object>>();
        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = outputList
                }
            },
            Inputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = inputList
                }
            }
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
        };
        var assertion = new DelayByAverage
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act + Assert
        Assert.Throws<EmptyInputListException>(() =>
            assertion.Assert(sessionList, new ImmutableArray<DataSource>()));
    }


    [Test]
    public void
        TestAssertSingleSession_CallFunctionWithOutputListThatHasEarlierInsertionTimeThanInputListAndWillCreateBiggerNegativeDelayThanTheNegativeDelayBuffer_ShouldRaiseException
        ()
    {
        // Arrange
        const long maximumDelayMilliSeconds = 2000; // 2 seconds
        const string name = "Test";
        var incorrectOutputList = new List<DetailedData<object>>
        {
            new() {Body = null, Timestamp = new DateTime(0)},
            new() {Body = null, Timestamp = new DateTime(0)},
            new() {Body = null, Timestamp = new DateTime(0)},
        };

        var incorrectInputList = new List<DetailedData<object>>
        {
            new() {Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000)},
            new() {Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000)},
            new() {Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000)}
        };
        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = incorrectOutputList
                }
            },
            Inputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = incorrectInputList
                }
            }
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = maximumDelayMilliSeconds/2
        };
        var assertion = new DelayByAverage
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act

        // Assert
        Assert.Throws<NegativeDelayException>(() =>
            assertion.Assert(sessionList, new ImmutableArray<DataSource>()));
    }
    
    [Test]
    public void
        TestAssertSingleSession_CallFunctionWithOutputListThatHasEarlierInsertionTimeThanInputListAndWillCreateSmallerNegativeDelayThanTheNegativeDelayBuffer_ShouldReturnTrue
        ()
    {
        // Arrange
        const long maximumDelayMilliSeconds = 2000; // 2 seconds
        const string name = "Test";
        var incorrectOutputList = new List<DetailedData<object>>
        {
            new() {Body = null, Timestamp = new DateTime(0)},
            new() {Body = null, Timestamp = new DateTime(0)},
            new() {Body = null, Timestamp = new DateTime(0)},
        };

        var incorrectInputList = new List<DetailedData<object>>
        {
            new() {Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000)},
            new() {Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000)},
            new() {Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000)}
        };

        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = incorrectOutputList
                }
            },
            Inputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = incorrectInputList
                }
            }
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = maximumDelayMilliSeconds * 2
        };
        var assertion = new DelayByAverage
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };

        // Act
        var result = assertion.Assert(sessionList, new ImmutableArray<DataSource>());
        
        // Assert
        Assert.That(result);
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithNoOutputs_ShouldReturnTrueWithNoDelayMessage()
    {
        const string name = "Test";
        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = new DateTime(1) }
                    }
                }
            },
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>()
                }
            }
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 1000
            }
        };

        var result = assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty);

        Assert.That(result, Is.True);
        StringAssert.Contains("No outputs found", assertion.AssertionMessage!);
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithInputWithoutTimestamp_ShouldThrowNotSupportedException()
    {
        const string name = "Test";
        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = null }
                    }
                }
            },
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = new DateTime(1) }
                    }
                }
            }
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 1000
            }
        };

        Assert.Throws<NotSupportedException>(() =>
            assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty));
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithOutputWithoutTimestamp_ShouldThrowNotSupportedException()
    {
        const string name = "Test";
        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = new DateTime(1) }
                    }
                }
            },
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = null }
                    }
                }
            }
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 1000
            }
        };

        Assert.Throws<NotSupportedException>(() =>
            assertion.Assert(new List<SessionData> { session }.ToImmutableList(), ImmutableList<DataSource>.Empty));
    }
}
