using System.Collections.Immutable;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Hermetic;

/// <summary>
/// Performs a hermetic test by comparing the the count of a specified input with multiplied by given percentage modifier
/// to the count of a specified output
/// </summary>
public class HermeticByInputOutputPercentage: BaseAssertion<HermeticByInputOutputPercentageConfiguration>
{
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var inputCount = Configuration.InputNames!.Sum(input =>
            Configuration.InputsAreOutputs
                ? sessionDataList.Sum(sessionData =>
                    sessionData.TryGetOutputByName(input, out var data) ? data!.Data.Count : 0)
                : sessionDataList.Sum(sessionData =>
                    sessionData.TryGetInputByName(input, out var data) ? data!.Data.Count : 0));
        var outputCount = Configuration.OutputNames!.Sum(output => sessionDataList.Sum(sessionData =>
            sessionData.TryGetOutputByName(output, out var data) ? data!.Data.Count : 0));

        var actualPercentage = inputCount == 0
            ? outputCount == 0 ? 0 : double.PositiveInfinity
            : (double)outputCount * 100 / inputCount;
        AssertionMessage = $"Sum of outputs {string.Join(", ", Configuration.OutputNames!)} count is {outputCount}, " +
                           $"The sum of the inputs { string.Join(", ", Configuration.InputNames!)} count is {inputCount}, " +
                           $"The percentage between the total output count and total input count is {actualPercentage}";
        return outputCount == 
               (int)Math.Round(inputCount*(Configuration.ExpectedPercentage!.Value/100), Configuration.MidpointRounding);
    }
}
