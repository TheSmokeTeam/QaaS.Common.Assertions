using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
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
public class DelayByChunksTests
{
    private static readonly MethodInfo? MultipleInputsToMultipleOutputsChunkCountMethod = typeof(DelayByChunks)
        .GetMethod("MultipleInputsToMultipleOutputsChunkCount", 
            BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? GetListOfChunkTimesMethod = typeof(DelayByChunks)
        .GetMethod("GetListOfChunkTimes", 
        BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(typeof(byte[]));

    private static IEnumerable<TestCaseData> TestGetListOfChunkTimesCaseSource()
    {
        const long baseDateTimeTicks = 10_000_000_000_000;
        var detailedDataList = new List<DetailedData<byte[]>>
        {
            new (){Body = Array.Empty<byte>(), Timestamp = new DateTime(
                baseDateTimeTicks + 0)},
            new (){Body = Array.Empty<byte>(), Timestamp = new DateTime(
                baseDateTimeTicks + 2)},
            new (){Body = Array.Empty<byte>(), Timestamp = new DateTime(
                baseDateTimeTicks + 4)},
            new (){Body = Array.Empty<byte>(), Timestamp = new DateTime(
                baseDateTimeTicks + 6)},
            new (){Body = Array.Empty<byte>(), Timestamp = new DateTime(
                baseDateTimeTicks + 8)},
            new (){Body = Array.Empty<byte>(), Timestamp = new DateTime(
                baseDateTimeTicks + 10)}
        };
        yield return new TestCaseData(2, ChunkTimeOption.Average, detailedDataList, new List<DateTime>()
        {
            new(baseDateTimeTicks + 1),
            new(baseDateTimeTicks + 5),
            new(baseDateTimeTicks + 9)
        }).SetName("WithAverageChunkTimeOption");
        yield return new TestCaseData(2, ChunkTimeOption.First, detailedDataList, new List<DateTime>()
        {
            new(baseDateTimeTicks + 0),
            new(baseDateTimeTicks + 4),
            new(baseDateTimeTicks + 8)
        }).SetName("WithFirstChunkTimeOption");
        yield return new TestCaseData(2, ChunkTimeOption.Last, detailedDataList, new List<DateTime>()
        {
            new(baseDateTimeTicks + 2),
            new(baseDateTimeTicks + 6),
            new(baseDateTimeTicks + 10)
        }).SetName("WithLastChunkTimeOption");
    }
    
    [Test, TestCaseSource(nameof(TestGetListOfChunkTimesCaseSource))]
    public void
        TestGetListOfChunkTimes_CallFunctionWithDifferentChunkTimeOptions_ShouldOutputExpectedTimeList(
            int chunkSize, ChunkTimeOption chunkTimeOption, List<DetailedData<byte[]>> list, List<DateTime> expectedOutputList)
    {
        // Act
        var chunkTimesList = (List<DateTime>)GetListOfChunkTimesMethod!.Invoke(null, 
            new object?[] { list, chunkSize, chunkTimeOption })!;
        
        // Assert
        CollectionAssert.AreEqual(expectedOutputList.Select(time => (long)(time - new DateTime(0)).TotalMilliseconds), 
            chunkTimesList.Select(time => (long)(time - new DateTime(0)).TotalMilliseconds));
    }
    
    [Test,
    TestCase(1, 0, ChunkTimeOption.Average),
    TestCase(1, 0, ChunkTimeOption.First),
    TestCase(1, 0, ChunkTimeOption.Last),
    TestCase(1, 1, ChunkTimeOption.Average),
    TestCase(1, 1, ChunkTimeOption.First),
    TestCase(1, 1, ChunkTimeOption.Last),
    TestCase(4, 1, ChunkTimeOption.Average),
    TestCase(4, 1, ChunkTimeOption.First),
    TestCase(4, 1, ChunkTimeOption.Last),
    TestCase(1, 16, ChunkTimeOption.Average),
    TestCase(1, 16, ChunkTimeOption.First),
    TestCase(1, 16, ChunkTimeOption.Last),
    TestCase(4, 16, ChunkTimeOption.Average),
    TestCase(4, 16, ChunkTimeOption.First),
    TestCase(4, 16, ChunkTimeOption.Last)]
    public void
        TestGetListOfChunkTimes_CallFunctionWithDifferentChunkTimeOptionsChunkSizesAndListSizes_ShouldAlwaysOutputTheExpectedAmountOfTimeStamps(
            int chunkSize, int listSize, ChunkTimeOption chunkTimeOption)
    {
        // Arrange
        const long baseDateTimeTicks = 10_000_000_000_000;
        var list = new List<DetailedData<byte[]>>();
        for (var listIndex = 0; listIndex < listSize; listIndex++)
        {
            list.Add(new DetailedData<byte[]>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicks + listIndex)});
        }
        
        // Act
        var chunkTimesList = (List<DateTime>)GetListOfChunkTimesMethod!.Invoke(null, new object?[] { list, chunkSize, chunkTimeOption })!;
        
        // Assert
        Assert.AreEqual(listSize/chunkSize, chunkTimesList.Count);
    }
    
    [Test,
    TestCase(2000, 3, 6, 2, 4, 0, 4000),
    TestCase(2000, 3, 6, 2, 4, 2, 1000),
    TestCase(2000, 3, 6, 2, 4, 2, -50),
    TestCase(2000, 3, 6, 2, 4, 2, -2000, 2500)]
    public void
        TestMultipleInputsToMultipleOutputsChunkCount_CallFunctionWithDifferentParameters_ShouldGetTheSameAsExpectedOutputChunksCountVariable(
            long maximumDelayMilliSeconds, int inputChunkSize, int inputListSize, int outputChunkSize,
            int outputListSize, int expectedOutputChunksCount,
            int timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds, int maximumNegativeDelay = 100)
    {
        // Arrange
        const string name = "test";
        const long baseDateTimeTicksToNotReachNegativeDelay = 10_000_000_000_000;
        var inputList = new List<DetailedData<object>>();
        for (var index = 0; index < inputListSize; index++)
        {
            inputList.Add(new DetailedData<object>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicksToNotReachNegativeDelay + index)});
        }

        var outputList = new List<DetailedData<object>>();
        for (var index = 0; index < outputListSize; index++)
        {
            outputList.Add(new DetailedData<object>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicksToNotReachNegativeDelay + index +
                timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds * 10_000)});
        }
        
        var configurations = new DelayByChunksConfiguration
        {
            Output = new Chunk
            {
                Name = name ,
                ChunkSize = outputChunkSize
            },
            Input = new Chunk
            {
                Name = name,
                ChunkSize = inputChunkSize
            },
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = maximumNegativeDelay
        };
        var assertion = new DelayByChunks
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act
        var testResult =
            (int)MultipleInputsToMultipleOutputsChunkCountMethod!.Invoke(assertion, new object?[] { inputList, outputList })!;

        // Assert
        Assert.AreEqual(expectedOutputChunksCount, testResult);
    }
    
    [Test]
    public void TestAssertSingleSession_CallFunctionWithEmptyInputList_FunctionShouldThrowException()
    {
        // Arrange
        const string name = "test";
        const long maximumDelayMilliSeconds = 2000; // 2 seconds
        const int inputChunkSize = 2;
        const int outputChunkSize = 3;
        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = new List<DetailedData<object>>() }
            },
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = new List<DetailedData<object>>() }
            }
        };
        var configurations = new DelayByChunksConfiguration
        {
            Output = new Chunk
            {
                Name = name,
                ChunkSize = outputChunkSize
            },
            Input = new Chunk
            {
                Name = name,
                ChunkSize = inputChunkSize
            },
            MaximumDelayMs = maximumDelayMilliSeconds
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var assertion = new DelayByChunks
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act

        // Assert
        Assert.Throws<EmptyInputListException>(() =>
           assertion.Assert(sessionList, new ImmutableArray<DataSource>()));
    }
    
    [Test]
    [TestCase(2000,2,4,3,6,2000)]
    [TestCase(5000,65,32500,97,48500,2500)]
    [TestCase(2000,3,3,2,2,2000)]
    public void
    TestAssertSingleSession_CallFunctionWithDifferentParametersWithNegativeDelayAndWithNegativeDelayBuffer0_ShouldThrowNegativeDelayException(
        long maximumDelayMilliSeconds, int inputChunkSize, int inputListSize, int outputChunkSize,
        int outputListSize,
        int timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds)
    {
        // Arrange
        const string name = "test";
        const long baseDateTimeTicksToNotReachNegativeDelay = 10_000_000_000_000;
        var outputList = new List<DetailedData<object>>();
        for (var index = 0; index < outputListSize; index++)
        {
            outputList.Add(new DetailedData<object>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicksToNotReachNegativeDelay + index)});
        }
        
        var inputList = new List<DetailedData<object>>();
        for (var index = 0; index < inputListSize; index++)
        {
            inputList.Add(new DetailedData<object>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicksToNotReachNegativeDelay + index +
                timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds * 10_000)});
        }
        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = inputList }
            },
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = outputList }
            }
        };
        var sessionList = new List<SessionData>()
        {
            session
        }.ToImmutableList();
        var configurations = new DelayByChunksConfiguration
        {
            Output = new Chunk
            {
                Name = name,
                ChunkSize = outputChunkSize
            },
            Input = new Chunk
            {
                Name = name,
                ChunkSize = inputChunkSize
            },
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = 0
        };
        var assertion = new DelayByChunks
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act + Assert
        Assert.Throws<NegativeDelayException>(() =>
            assertion.Assert(sessionList, new ImmutableArray<DataSource>()));
    }
    
    [Test]
    [TestCase(2000, 1, 3, 1, 3, true, 1000)]
    [TestCase(2000, 2, 4, 0, 6, true, 1000)]
    [TestCase(2000, 2, 4, 3, 0, false, 1000)]
    [TestCase(2000, 2, 4, 0, 0, true, 1000)]
    [TestCase(2000, 2, 4, 3, 6, true, 1000)]
    [TestCase(2000, 2, 6, 3, 7, false, 1000)]
    [TestCase(2000, 2, 6, 3, 10, true, 1000)]
    [TestCase(2000, 2, 5, 3, 9, true, 1000)]
    [TestCase(2000, 2, 7, 3, 9, true, 1000)]
    [TestCase(2000, 3, 15, 1, 5, true, 1000)]
    [TestCase(2000, 1, 5, 3, 15, true, 1000)]
    [TestCase(2000, 2, 4, 3, 6, false, 4000)]
    [TestCase(8000, 65, 32500, 97, 48500, true, 2500)]
    [TestCase(5000, 65, 32500, 97, 36400, false, 2500)]
    [TestCase(5000, 65, 26000, 97, 48500, true, 2500)]
    [TestCase(2000, 3, 15, 1, 5, true, -50)]
    [TestCase(2000, 3, 15, 1, 5, true, -2000, 2500)]
    public void
        TestChunksArriveOnTime_CallFunctionUsingDetailedMessageListsWithDifferentParameters_ShouldReturnTheSameAsShouldArriveInTimeVariable
        (long maximumDelayMilliSeconds, int inputChunkSize, int inputListSize, int outputChunkSize,
            int outputListSize, bool shouldArriveInTime,
            int timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds, int maximumAllowedNegativeDelayMs = 100)
    {
        // Arrange
        const string name = "test";
        const long baseDateTimeTicksToNotReachNegativeDelay = 10_000_000_000_000;
        
        var inputList = new List<DetailedData<object>>();
        for (var index = 0; index < inputListSize; index++)
        {
            inputList.Add(new DetailedData<object>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicksToNotReachNegativeDelay + index)});
        }
        var outputList = new List<DetailedData<object>>();
        for (var index = 0; index < outputListSize; index++)
        {
            outputList.Add(new DetailedData<object>{Body = null, Timestamp = new DateTime(
                baseDateTimeTicksToNotReachNegativeDelay + index +
                timeDifferenceBetweenCreatingInputAndOutputListsMilliSeconds * 10_000)});
        }
        var session = new SessionData
        {
            Name = "Id",
            Inputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = inputList }
            },
            Outputs = new List<CommunicationData<object>>
            {
                new() { Name = name, Data = outputList }
            }
        };
        var sessionList = new List<SessionData>()
        {
            session
        }.ToImmutableList();
        var configurations = new DelayByChunksConfiguration
        {
            Output = new Chunk
            {
                Name = name,
                ChunkSize = outputChunkSize
            },
            Input = new Chunk
            {
                Name = name,
                ChunkSize = inputChunkSize
            },
            MaximumDelayMs = maximumDelayMilliSeconds,
            MaximumNegativeDelayBufferMs = maximumAllowedNegativeDelayMs
        };
        var assertion = new DelayByChunks
        {
            Context = new Context{Logger = Globals.Logger},
            Configuration = configurations
        };
        
        // Act
        var testResult = assertion.Assert(sessionList, new ImmutableArray<DataSource>());

        // Assert
        Assert.That(testResult == shouldArriveInTime);
        if (outputChunkSize > 0 && outputListSize > 0)
            StringAssert.Contains($"Expected {inputList.Count / inputChunkSize} output chunks" +
                              $" to arrive in under {maximumDelayMilliSeconds} milliseconds", 
            assertion.AssertionMessage);
    }
}