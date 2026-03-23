using System.Collections.Immutable;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Hermetic;

/// <summary>
/// Checks whether the percentage between configured inputs and outputs stays within the expected minimum and maximum range.
/// </summary>
public class HermeticByInputOutputPercentageInRange : BaseAssertion<HermeticByInputOutputPercentageInRangeConfiguration>
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
        AssertionMessage =
            $"The sum of the count of the outputs {string.Join(", ", Configuration.OutputNames!)} is {outputCount}, " +
            $"The sum of the count of the inputs {string.Join(", ", Configuration.InputNames!)} is {inputCount}. " +
            $"The percentage between the total output count and total input count is {actualPercentage}, " +
            $"The expected maximum percentage: {Configuration.ExpectedMaximumPercentage}, " +
            $"The expected minimum percentage: {Configuration.ExpectedMinimumPercentage}. ";
        return Configuration.ExpectedMinimumPercentage <= actualPercentage &&
               Configuration.ExpectedMaximumPercentage >= actualPercentage;
    }
}
