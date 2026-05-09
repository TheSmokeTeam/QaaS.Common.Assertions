using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    public void TestAssertSingleSession_CallFunctionWithASingleSession_ShouldReturnTheSameAsShouldArriveInTimeVariable(
        long maximumDelayMilliSeconds,
        int inputListSize,
        int outputListSize,
        bool shouldArriveInTime,
        int timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds
    )
    {
        // Arrange
        const string name = "Test";
        var inputList = new List<DetailedData<object>>(inputListSize);
        for (var index = 0; index < inputListSize; index++)
        {
            inputList.Add(
                new DetailedData<object> { Body = null, Timestamp = new DateTime(index) }
            );
        }
        var input = new CommunicationData<object> { Name = name, Data = inputList };

        var outputList = new List<DetailedData<object>>(outputListSize);
        for (var index = 0; index < outputListSize; index++)
        {
            outputList.Add(
                new DetailedData<object>
                {
                    Body = null,
                    Timestamp = new DateTime(
                        index
                            + timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds * 10_000
                    ),
                }
            );
        }
        var output = new CommunicationData<object> { Name = name, Data = outputList };

        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>> { output },
            Inputs = new List<CommunicationData<object>> { input },
        };
        var sessionList = new List<SessionData> { session }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations,
        };

        // Act
        var testResult = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.That(testResult == shouldArriveInTime);
        if (outputListSize > 0)
            StringAssert.Contains(
                $"maximum allowed average delay is {maximumDelayMilliSeconds} milliseconds",
                assertion.AssertionMessage!
            );
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithWithEmptyInputListAndOutputListInSession_ShouldGetExceptionFromFunction()
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
                new() { Name = name, Data = outputList },
            },
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = inputList },
            },
        };
        var sessionList = new List<SessionData> { session }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations,
        };

        // Act + Assert
        Assert.Throws<EmptyInputListException>(() =>
            assertion.Assert(sessionList, new ImmutableArray<DataSource>())
        );
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithOutputListThatHasEarlierInsertionTimeThanInputListAndWillCreateBiggerNegativeDelayThanTheNegativeDelayBuffer_ShouldRaiseException()
    {
        // Arrange
        const long maximumDelayMilliSeconds = 2000; // 2 seconds
        const string name = "Test";
        var incorrectOutputList = new List<DetailedData<object>>
        {
            new() { Body = null, Timestamp = new DateTime(0) },
            new() { Body = null, Timestamp = new DateTime(0) },
            new() { Body = null, Timestamp = new DateTime(0) },
        };

        var incorrectInputList = new List<DetailedData<object>>
        {
            new() { Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000) },
            new() { Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000) },
            new() { Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000) },
        };
        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = incorrectOutputList },
            },
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = incorrectInputList },
            },
        };
        var sessionList = new List<SessionData> { session }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = maximumDelayMilliSeconds / 2,
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations,
        };

        // Act

        // Assert
        Assert.Throws<NegativeDelayException>(() =>
            assertion.Assert(sessionList, new ImmutableArray<DataSource>())
        );
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithOutputListThatHasEarlierInsertionTimeThanInputListAndWillCreateSmallerNegativeDelayThanTheNegativeDelayBuffer_ShouldReturnTrue()
    {
        // Arrange
        const long maximumDelayMilliSeconds = 2000; // 2 seconds
        const string name = "Test";
        var incorrectOutputList = new List<DetailedData<object>>
        {
            new() { Body = null, Timestamp = new DateTime(0) },
            new() { Body = null, Timestamp = new DateTime(0) },
            new() { Body = null, Timestamp = new DateTime(0) },
        };

        var incorrectInputList = new List<DetailedData<object>>
        {
            new() { Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000) },
            new() { Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000) },
            new() { Body = null, Timestamp = new DateTime(maximumDelayMilliSeconds * 10_000) },
        };

        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = incorrectOutputList },
            },
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = incorrectInputList },
            },
        };
        var sessionList = new List<SessionData> { session }.ToImmutableList();
        var configurations = new DelayByAverageConfiguration
        {
            OutputName = name,
            InputName = name,
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = maximumDelayMilliSeconds * 2,
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations,
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
                        new() { Body = null, Timestamp = new DateTime(1) },
                    },
                },
            },
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = new List<DetailedData<object>>() },
            },
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 1000,
            },
        };

        var result = assertion.Assert(
            new List<SessionData> { session }.ToImmutableList(),
            ImmutableList<DataSource>.Empty
        );

        Assert.That(result, Is.True);
        StringAssert.Contains("No outputs found", assertion.AssertionMessage!);
    }

    [Test]
    public void TestAssertSingleSession_CallFunctionWithInputsAreOutputs_ShouldUseConfiguredOutputAsInputSource()
    {
        const string inputAsOutputName = "inputAsOutput";
        const string outputName = "result";
        var session = new SessionData
        {
            Name = "Id",
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = inputAsOutputName,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = new DateTime(10_000) },
                        new() { Body = null, Timestamp = new DateTime(20_000) },
                    },
                },
                new()
                {
                    Name = outputName,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = new DateTime(20_000) },
                        new() { Body = null, Timestamp = new DateTime(30_000) },
                    },
                },
            },
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = inputAsOutputName,
                InputsAreOutputs = true,
                OutputName = outputName,
                MaximumDelayMs = 1,
            },
        };

        var result = assertion.Assert(
            new List<SessionData> { session }.ToImmutableList(),
            ImmutableList<DataSource>.Empty
        );

        Assert.That(result, Is.True);
        StringAssert.Contains(
            "Average Delay between all inputs to all outputs",
            assertion.AssertionMessage!
        );
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
                        new() { Body = null, Timestamp = null },
                    },
                },
            },
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = new DateTime(1) },
                    },
                },
            },
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 1000,
            },
        };

        Assert.Throws<NotSupportedException>(() =>
            assertion.Assert(
                new List<SessionData> { session }.ToImmutableList(),
                ImmutableList<DataSource>.Empty
            )
        );
    }

    [Test]
    public void TestAssertSingleSession_PrecisionNotLostByPerItemTruncation_ShouldComputeExactMillisecondAverage()
    {
        // C-15: summing (ticks/ms) per-item truncates each item before accumulating, losing up to 1 ms per item.
        // The correct path sums raw ticks, divides once at the end.
        // Construct 3 inputs whose per-item truncation would give a wrong answer but raw-tick sum gives the right one.
        // Input ticks: 1_499_999, 1_499_999, 1_499_999 (each is 0 ms when truncated per-item, but average should be 0 ms too)
        // Use ticks that straddle the ms boundary so the difference matters:
        // 3 inputs at ticks 0, 1, 0 => raw sum = 1, avg = 0 ticks => 0 ms
        // 3 outputs at ticks 10_000, 10_001, 10_000 => raw sum = 30_001, avg = 10_000 ticks => 1 ms
        // Expected delay = 1 ms  (if we truncate per-item: each input=0ms, avg=0ms; each output=1ms, avg=1ms => 1ms — same here)
        // Use a case where the old code would differ: 3 inputs at 9_999, 9_999, 9_999 (truncated: 0 ms each)
        // raw sum = 29_997 ticks => 29_997 / 3 / 10_000 = 0 ms -- same result
        // Better: 3 inputs with ticks 19_999, 19_999, 19_999 => each truncates to 1 ms (sum = 3 ms, avg = 1 ms OLD)
        //         raw sum = 59_997; 59_997/3=19_999 ticks; 19_999/10_000=1 ms (NEW — same in this case)
        // The bug manifests when individual items round differently from the aggregate.
        // Example: inputs at ticks 5_000, 15_000 => per-item: 0+1=1, avg=0 ms (OLD integer div)
        //          raw sum=20_000; 20_000/2=10_000 ticks; /10_000=1 ms (CORRECT)
        // outputs at ticks 25_000, 35_000 => per-item: 2+3=5, avg=2 ms (OLD)
        //          raw sum=60_000; 60_000/2=30_000 ticks; /10_000=3 ms (CORRECT)
        // OLD delay = 2-0 = 2 ms. CORRECT delay = 3-1 = 2 ms. Same in this example too.
        // Use: inputs 1, 1 (ticks) => raw avg=1 tick=0 ms; OLD per-item 0+0=0 avg=0ms => same
        // The truncation difference: inputs at ticks 9_999 and 10_001 (only 2 items)
        // per-item truncation: 0+1=1, avg=0 (int div by 2)
        // raw: 19_000+10_001=19_000+10_001 -- wait, let me use TicksPerMillisecond=10_000
        // inputs: 9_999 and 10_001 ticks => per-item truncated ms: 0 and 1, sum=1, avg=0 (integer)
        // raw ticks sum=20_000; 20_000/2=10_000 ticks; 10_000/10_000=1 ms => different!
        // outputs: same as inputs but +20_000 ticks each: 29_999 and 30_001
        // per-item ms: 2 and 3, sum=5, avg=2 ms (OLD)
        // raw: 60_000/2=30_000 ticks; /10_000=3 ms (NEW)
        // OLD delay: 2-0=2ms. NEW delay: 3-1=2ms. Same answer :(
        // Let's engineer a case where the answer actually differs:
        // 1 input at 14_999 ticks => OLD ms=1, NEW ms=1. OK not here.
        // Need: N items where floor(sum(xi)/N/T) != floor(sum(floor(xi/T))/N)
        // 3 inputs: 5_000, 5_000, 20_001 ticks
        // per-item ms: 0, 0, 2 => sum=2, /3=0 ms OLD
        // raw sum=30_001 ticks; /3=10_000; /10_000=1 ms NEW  ← DIFFERENT!
        const string name = "Test";
        // Using raw tick values (1 tick = 100ns, TicksPerMillisecond=10000)
        var inputTicks = new long[] { 5_000, 5_000, 20_001 };
        var outputTicks = new long[] { 30_001, 30_001, 30_001 }; // avg=30_001/10_000=3ms

        var inputList = inputTicks
            .Select(t => new DetailedData<object> { Body = null, Timestamp = new DateTime(t) })
            .ToList();
        var outputList = outputTicks
            .Select(t => new DetailedData<object> { Body = null, Timestamp = new DateTime(t) })
            .ToList();

        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = inputList },
            },
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = outputList },
            },
        };

        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 10_000,
            },
        };

        var result = assertion.Assert(
            new List<SessionData> { session }.ToImmutableList(),
            ImmutableList<DataSource>.Empty
        );

        // NEW (correct): avg input = 30001/3 ticks /10000 = 1 ms; avg output = 90003/3 ticks /10000 = 3 ms; delay=2ms
        // OLD (buggy):   avg input = (0+0+2)/3 = 0 ms; avg output = (3+3+3)/3 = 3 ms; delay=3ms
        // Both pass the 10_000ms threshold, but we verify the assertion message contains the correct delay.
        Assert.That(result, Is.True);
        // Average delay should be 2 ms with correct tick-first arithmetic, not 3 ms.
        StringAssert.Contains("2 milliseconds", assertion.AssertionMessage!);
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
                        new() { Body = null, Timestamp = new DateTime(1) },
                    },
                },
            },
            Outputs = new List<CommunicationData<object>>
            {
                new()
                {
                    Name = name,
                    Data = new List<DetailedData<object>>
                    {
                        new() { Body = null, Timestamp = null },
                    },
                },
            },
        };
        var assertion = new DelayByAverage
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = new DelayByAverageConfiguration
            {
                InputName = name,
                OutputName = name,
                MaximumDelayMs = 1000,
            },
        };

        Assert.Throws<NotSupportedException>(() =>
            assertion.Assert(
                new List<SessionData> { session }.ToImmutableList(),
                ImmutableList<DataSource>.Empty
            )
        );
    }
}
