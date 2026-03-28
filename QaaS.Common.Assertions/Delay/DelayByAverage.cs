using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Microsoft.Extensions.Logging;
using MoreLinq;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;
using QaaS.Common.Assertions.Delay.Exceptions;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Delay;

/// <summary>
/// Checks for delay between an input source to an output source by
/// subtracting the average timestamp of all inputs from the average timestamp of all the outputs.
/// </summary>
/// <qaas-docs group="Latency" subgroup="Average latency" />
public class DelayByAverage: BaseAssertion<DelayByAverageConfiguration>
{
    private const string DateTimeLogFormat = "yyyy-MM-dd HH:mm:ss.fff";
    private readonly StringBuilder _traceStringBuilder = new();
    
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var sessionData = sessionDataList.AsSingle();
        var output = sessionData.GetOutputByName(Configuration.OutputName!);
        var input = Configuration.InputsAreOutputs
            ? sessionData.GetOutputByName(Configuration.InputName!)
            : sessionData.GetInputByName(Configuration.InputName!);
        var outputCount = output.Data.Count;
        var inputCount = input.Data.Count;
         
        // If there is no input delay cannot be measured and an exception is thrown
        if (inputCount == 0) throw new EmptyInputListException($"No input items were found in input {input.Name}," +
                                                               " cannot measure delay when no input was provided");
        // If there no output arrived delay passes automatically
        if (outputCount == 0)
        {
            AssertionMessage = $"No outputs found in {output.Name}, meaning there was no delay to be measured";
            return true;
        }

        // Get average input injection time (in milliseconds)
         BigInteger averageInputTime = 0;
         input.Data.ForEach(item => averageInputTime += !item.Timestamp.HasValue ?
             throw new NotSupportedException("Encountered input with no timestamp, can't calculate delay. " +
                                             $"\n Input with no timestamp: {item.Body}") : 
             item.Timestamp.Value.Ticks / TimeSpan.TicksPerMillisecond);
         averageInputTime /= inputCount;
         Context.Logger.LogDebug("Average input time is {AverageInputTime}",
             new DateTime(TimeSpan.FromMilliseconds((double)averageInputTime).Ticks).ToString(DateTimeLogFormat));

         BigInteger averageOutputTime = 0;
         output.Data.ForEach(item => averageOutputTime += !item.Timestamp.HasValue ?
             throw new NotSupportedException("Encountered output with no timestamp, can't calculate delay. " +
                                             $"\n Output with no timestamp: {item.Body}") : 
             item.Timestamp.Value.Ticks  / TimeSpan.TicksPerMillisecond);
         averageOutputTime /= outputCount;
         Context.Logger.LogDebug("Average output time is {AverageOutputTime}",
             new DateTime(TimeSpan.FromMilliseconds((double)averageOutputTime).Ticks).ToString(DateTimeLogFormat));
         
         var averageDelayMilliseconds = averageOutputTime - averageInputTime;
         var negativeDelayBufferMilliSecondsAsNegativeNumber = -1 * Configuration.MaximumNegativeDelayBufferMs!.Value;
         // Cannot have negative delay
         if (averageDelayMilliseconds < negativeDelayBufferMilliSecondsAsNegativeNumber)
             throw new NegativeDelayException($"Average delay is {averageDelayMilliseconds} milliseconds, delay cannot be a negative number" +
                                           $" below the negative delay buffer which is {negativeDelayBufferMilliSecondsAsNegativeNumber} milliseconds!" +
                                           " Issue with either the output timestamps or the input timestamps");

         if (averageDelayMilliseconds < 0 &&
             averageDelayMilliseconds >= negativeDelayBufferMilliSecondsAsNegativeNumber)
             _traceStringBuilder.Append($"\nAverage delay is negative but falls within the negative delay buffer" +
                                        $" of {Configuration.MaximumNegativeDelayBufferMs!.Value} milliseconds," +
                                        $"delay is {averageDelayMilliseconds} milliseconds" +
                                        " treating delay as 0 millisecond delay\n");
         
         
         AssertionMessage = $"Average Delay between all inputs to all outputs is {averageDelayMilliseconds} milliseconds, " +
                                   $"maximum allowed average delay is {Configuration.MaximumDelayMs!.Value} milliseconds";
         AssertionTrace = _traceStringBuilder.ToString();
         return averageDelayMilliseconds <= Configuration.MaximumDelayMs!.Value;
    }
}
