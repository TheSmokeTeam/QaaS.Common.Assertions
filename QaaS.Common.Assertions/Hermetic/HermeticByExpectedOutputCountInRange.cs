using System.Collections.Immutable;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Hermetic;

/// <summary>
/// Performs a hermetic test by comparing the count of a given output in a session to a given expected minimum and maximum
/// </summary>
/// <qaas-docs group="Hermeticity" subgroup="Output count range" />
public class HermeticByExpectedOutputCountInRange : BaseAssertion<HermeticByExpectedOutputCountInRangeConfiguration>
{
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var outputCount = Configuration.OutputNames!.Sum(output => sessionDataList.Sum(sessionData =>
            sessionData.TryGetOutputByName(output, out var data) ? data!.Data.Count : 0));
        AssertionMessage = $"The sum of the count of the outputs {string.Join(", ", Configuration.OutputNames!)} is " +
                           $"{outputCount}. The maximum limit is {Configuration.ExpectedMaximumCount}, " +
                           $"The Minimum limit {Configuration.ExpectedMinimumCount}.";
        return Configuration.ExpectedMinimumCount <= outputCount && outputCount <= Configuration.ExpectedMaximumCount;
    }
}
