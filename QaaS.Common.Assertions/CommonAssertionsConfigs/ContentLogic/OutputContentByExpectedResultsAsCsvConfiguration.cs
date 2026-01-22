using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.ContentLogic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;

[Description(
  "Checks for all json items in a configured output that their content is valid according to a csv results file." +
  "The results file contains the expected value of fields in each json item and is provided from the configured DataSources." +
  "Expects the output items to be deserialized to any type of C# object that can be converted to Json." +
  "`DataSources`: Used for loading the csv results file." +
  "`Session Support`: Only supports a single session assertion."
 ), Display(Name = nameof(OutputContentByExpectedCsvResults))]
public record OutputContentByExpectedResultsAsCsvConfiguration : OutputContentByExpectedResultsConfiguration
{
}
