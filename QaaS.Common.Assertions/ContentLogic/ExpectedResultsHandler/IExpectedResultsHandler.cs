using QaaS.Framework.SDK.Session.DataObjects;

namespace QaaS.Common.Assertions.ContentLogic.ExpectedResultsHandler;

/// <summary>
/// Interface for expected results handlers. Handler uses to adapt between the external data which represents
/// the expected results and the format that is being used to compare the actual results to the expected in the
/// OutputContentByExpectedResults assertion.
/// </summary>
public interface IExpectedResultsHandler
{
    IList<Dictionary<string, object?>> DeserializeExpectedResults(IEnumerable<Data<object>> expectedResults);
}