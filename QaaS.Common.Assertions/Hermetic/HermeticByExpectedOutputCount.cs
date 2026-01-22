using System.Collections.Immutable;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Hermetic;

/// <summary>
/// Performs a hermetic test by comparing the count of a given output in a session to a given expected count
/// </summary>
public class HermeticByExpectedOutputCount : BaseAssertion<HermeticByExpectedOutputCountConfiguration>
{
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var outputCount = Configuration.OutputNames!.Sum(output => sessionDataList.Sum(sessionData =>
            sessionData.TryGetOutputByName(output, out var data) ? data!.Data.Count : 0));
        AssertionMessage = $"Sum of outputs {string.Join(", ", Configuration.OutputNames!)} count is " +
                           $"{outputCount}, expected {Configuration.ExpectedCount}";
        return outputCount == Configuration.ExpectedCount;
    }
}