using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.ExpectedResultsHandler;

namespace QaaS.Common.Assertions.ContentLogic;

public class OutputContentByExpectedCsvResults : BaseOutputContentByExpectedResults<OutputContentByExpectedResultsAsCsvConfiguration>
{
    protected override IExpectedResultsHandler BuildExpectedResultsHandler() => new CsvHandler();
}