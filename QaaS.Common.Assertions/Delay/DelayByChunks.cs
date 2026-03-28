using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Microsoft.Extensions.Logging;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;
using QaaS.Common.Assertions.Delay.Exceptions;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Delay;

/// <summary>
/// Checks for delay between an input source to an output source by
/// subtracting the timestamp of input chunks of a configured size from a timestamp of output
/// chunks of a configured size, takes the chunks in ascending order of the input/output lists.
/// </summary>
/// <qaas-docs group="Latency" subgroup="Chunk latency" />
public class DelayByChunks: BaseAssertion<DelayByChunksConfiguration>
{
    private const string DateTimeLogFormat = "yyyy-MM-dd HH:mm:ss.fff";
    private readonly StringBuilder _traceStringBuilder = new();
    
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var sessionData = sessionDataList.AsSingle();
        var output = sessionData.GetOutputByName(Configuration.Output!.Name!);
        var input = Configuration.InputsAreOutputs
            ? sessionData.GetOutputByName(Configuration.Input!.Name!)
            : sessionData.GetInputByName(Configuration.Input!.Name!);
        var outputCount = output.Data.Count;
        var inputCount = input.Data.Count;
         
        // If there is no input delay cannot be measured and an exception is thrown
        if (inputCount == 0) throw new EmptyInputListException($"No input items were found in input {input.Name}," +
                                                                    " cannot measure delay when no input was provided");
        // If there should be no output there is no delay to calculate and the "output" arrived on time
        if (Configuration.Output.ChunkSize == 0)
        {
            AssertionMessage = "Output ChunkSize was configured as 0, meaning there is no delay to calculate";
            return true;
        }
        // If there should be output but there is no output this immediately fails
        if (outputCount == 0)
        {
            AssertionMessage = $"No output items were found in the output {output.Name}" +
                                " are you sure your input produces output? if its not supposed to " +
                                "produce any output make sure to set Output.ChunkSize to 0";
            return false;
        }
        
        var outputChunksCount = MultipleInputsToMultipleOutputsChunkCount((IReadOnlyList<DetailedData<object>>)input.Data,
            (IReadOnlyList<DetailedData<object>>)output.Data);
        
        var expectedOutputChunkCount = inputCount / Configuration.Input.ChunkSize;
        AssertionMessage = $"Expected {expectedOutputChunkCount} output chunks" +
                           $" to arrive in under {Configuration.MaximumDelayMs} milliseconds," +
                           $" {outputChunksCount} actually arrived";
        AssertionTrace = _traceStringBuilder.ToString();
        return expectedOutputChunkCount == outputChunksCount;
    }

    /// <summary>
    /// Gets input and output lists with timestamps of every message and returns a count of all the messages
    /// that arrived from input to output in less than the given maximum Delay
    /// assumes component is sync and that the location of a message in the input list indicates
    /// its location in the output list
    /// meant to be used with the same inputs (that become a few outputs) injected in loops
    /// </summary>
    /// <param name="inputList">The list of inputs with timestamps</param>
    /// <param name="outputList">The list of outputs with timestamps</param>
    /// <returns>The number of output chunks that arrived on time</returns>
    private int MultipleInputsToMultipleOutputsChunkCount(IReadOnlyList<DetailedData<object>> inputList,
        IReadOnlyList<DetailedData<object>> outputList)
    {
        var chunkInputTimesList = GetListOfChunkTimes(inputList, Configuration.Input!.ChunkSize!.Value,
            Configuration.Input!.ChunkTimeOption);
        var chunkInputTimesListLength = chunkInputTimesList.Count;
        var chunkOutputTimesList = GetListOfChunkTimes(outputList, Configuration.Output!.ChunkSize!.Value,
            Configuration.Output!.ChunkTimeOption);
        var chunkOutputTimesListLength = chunkOutputTimesList.Count;

        if (chunkInputTimesListLength == 0)
        {
            _traceStringBuilder.Append(
                "\nNo complete input chunks were found, no delay comparison can be performed.\n");
            return 0;
        }

        if (chunkOutputTimesListLength == 0)
        {
            _traceStringBuilder.Append(
                "\nNo complete output chunks were found, no chunks arrived on time.\n");
            return 0;
        }

        // Add to trace the average delay of chunks
        BigInteger averageInputChunksTime = 0;
        chunkInputTimesList.ForEach(input => averageInputChunksTime += input.Ticks / TimeSpan.TicksPerMillisecond);
        averageInputChunksTime /= chunkInputTimesListLength;

        BigInteger averageOutputChunksTime = 0;
        chunkOutputTimesList.ForEach(input => averageOutputChunksTime += input.Ticks / TimeSpan.TicksPerMillisecond);
        averageOutputChunksTime /= chunkOutputTimesListLength;
        _traceStringBuilder.Append($"\nAverage output chunk time is {new DateTime(TimeSpan.FromMilliseconds((double)averageOutputChunksTime).Ticks).ToString(DateTimeLogFormat)} and " +
                                   $"average input chunk time is {new DateTime(TimeSpan.FromMilliseconds((double)averageInputChunksTime).Ticks).ToString(DateTimeLogFormat)} making " +
                                   $"the delay between all input and output chunks that arrived" +
                                   $" {averageOutputChunksTime - averageInputChunksTime} milliseconds\n");

        var negativeDelayBufferMilliSecondsAsNegativeNumber = -1 * Configuration.MaximumNegativeDelayBufferMs!.Value;
        // Start calculation to see how many chunks arrived on time
        var outputChunksArrivedOnTimeCount = 0;
        // Enumerable of all delays between input and output chunks
        for (var inputTimeStampIndex = 0;
             inputTimeStampIndex < chunkInputTimesListLength;
             inputTimeStampIndex++)
        {
            try
            {
                var delayMilliSeconds = (chunkOutputTimesList[inputTimeStampIndex]
                                         - chunkInputTimesList[inputTimeStampIndex]).TotalMilliseconds;

                // Cannot have negative delay
                if (delayMilliSeconds < negativeDelayBufferMilliSecondsAsNegativeNumber)
                {
                    throw new NegativeDelayException(
                        $"Chunk delay is {delayMilliSeconds} milliseconds, delay cannot be a negative number" +
                        $" below the negative delay buffer which is {negativeDelayBufferMilliSecondsAsNegativeNumber} milliseconds!" +
                        " Issue with either the output timestamps or the input timestamps");
                }

                if (delayMilliSeconds < 0 && delayMilliSeconds >= negativeDelayBufferMilliSecondsAsNegativeNumber)
                {
                    _traceStringBuilder.Append($"\nDelay is negative but falls within the negative delay buffer" +
                            $" of {Configuration.MaximumNegativeDelayBufferMs!.Value} milliseconds," +
                              $"delay is {delayMilliSeconds} milliseconds" +
                                " treating delay as 0 millisecond delay\n");
                    delayMilliSeconds = 0;
                }

                // Check I/O delay is below maximum allowed delay
                if (delayMilliSeconds <= Configuration.MaximumDelayMs!.Value)
                {
                    outputChunksArrivedOnTimeCount++;
                }

            }
            catch (ArgumentOutOfRangeException e)
            {
                Context.Logger.LogDebug("Exception Was Thrown : \n {Exception}", e);
                Context.Logger.LogInformation("Not all inputs arrived as outputs in time");
                break;
            }
        }

        return outputChunksArrivedOnTimeCount;
    }

    /// <summary>
    /// Creates list of date times from chunks of a List of detailed messages
    /// </summary>
    /// <param name="detailedDataList"></param>
    /// <param name="chunkSize"> the size of chunks to calculate time from in list of detailed messages,
    /// cannot be 0 or negative</param>
    /// <param name="chunkTimeOption"> How to create the chunk's time according to the enum options </param>
    /// <typeparam name="T"></typeparam>
    /// <returns> List of chunk dateTimes </returns>
    private static List<DateTime> GetListOfChunkTimes<T>(IReadOnlyList<DetailedData<T>> detailedDataList, 
        int chunkSize, ChunkTimeOption chunkTimeOption)
    {
        const string onTimeStampExceptionMessage = "DetailedData item has no timestamp, can't calculate delay";
        
        if (chunkSize <= 0) throw new ArgumentException(
            $"chunkSize cannot be 0 or less, the given chunkSize was {chunkSize}");
        
        var listLength = detailedDataList.Count;
        var listOfChunkTimes = new List<DateTime>();
        for(var listIndex = 0; listIndex < listLength; listIndex+= chunkSize)
        {
            if (listIndex + chunkSize > listLength)
            {
                break;
            }

            DateTime chunkTime;
            switch (chunkTimeOption)
            {
                case ChunkTimeOption.Average:
                    var chunkList = new List<DateTime>(); 
                    for(var chunkIndex = 0; chunkIndex < chunkSize; chunkIndex++)
                    {
                        var itemInChunk = detailedDataList[listIndex + chunkIndex].Timestamp;
                        chunkList.Add(itemInChunk ?? throw new NotSupportedException(onTimeStampExceptionMessage));
                    }
            
                    chunkTime = DateTime.MinValue.AddSeconds(chunkList.Sum(time =>
                        (time - DateTime.MinValue).TotalSeconds) / chunkList.Count);
                    break;
                
                case ChunkTimeOption.First:
                    chunkTime = detailedDataList[listIndex].Timestamp ??
                                throw new NotSupportedException(onTimeStampExceptionMessage);
                    break;
                
                case ChunkTimeOption.Last:
                    chunkTime = detailedDataList[listIndex + chunkSize - 1].Timestamp ?? 
                                throw new NotSupportedException(onTimeStampExceptionMessage);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(
                        $"ChunkTimeOption {chunkTimeOption} is not supported");
            }
            listOfChunkTimes.Add(chunkTime);
        }
        return listOfChunkTimes;
    }
}
