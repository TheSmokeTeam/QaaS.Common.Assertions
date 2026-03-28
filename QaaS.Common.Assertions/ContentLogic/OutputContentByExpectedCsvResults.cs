using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.ExpectedResultsHandler;

namespace QaaS.Common.Assertions.ContentLogic;

/// <summary>
/// Checks that the configured output content matches the expected values loaded from a CSV results file.
/// </summary>
/// <qaas-docs group="Content validation" subgroup="CSV-driven field validation" />
public class OutputContentByExpectedCsvResults : BaseOutputContentByExpectedResults<OutputContentByExpectedResultsAsCsvConfiguration>
{
    protected override IExpectedResultsHandler BuildExpectedResultsHandler() => new CsvHandler();
}
