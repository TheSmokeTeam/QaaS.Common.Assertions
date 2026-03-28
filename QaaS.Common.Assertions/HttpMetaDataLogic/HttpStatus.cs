using System.Collections.Immutable;
using QaaS.Common.Assertions.CommonAssertionsConfigs.HttpMetaDataLogic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.HttpMetaDataLogic;

/// <summary>
/// Performs a logic test on the http status of all selected outputs in a session by checking they all have the desired http status code
/// </summary>
/// <qaas-docs group="Content validation" subgroup="HTTP status" />
public class HttpStatus: BaseAssertion<HttpStatusConfiguration>
{
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var sessionData = sessionDataList.AsSingle();
        var outputs = Configuration.OutputNames!.Select(output => sessionData.GetOutputByName(output));
    
        // Count all output items with incorrect status and the incorrect statuses themselves and the output items with no status
        var itemsWithNoStatusPerOutput = new Dictionary<string, int>();
        var itemsWithIncorrectStatusPerOutput = new Dictionary<string, int>();
        var incorrectStatusesCount = new Dictionary<int, int>();
        foreach (var output in outputs)
        {
            itemsWithNoStatusPerOutput[output.Name] = 0;
            itemsWithIncorrectStatusPerOutput[output.Name] = 0;
                foreach (var item in output.Data)
                {
                    if (item.MetaData?.Http?.StatusCode == null)
                        itemsWithNoStatusPerOutput[output.Name]++;
                    else if (item.MetaData.Http.StatusCode.Value != Configuration.StatusCode!.Value)
                    {
                        itemsWithIncorrectStatusPerOutput[output.Name]++;
                        if (!incorrectStatusesCount.ContainsKey(item.MetaData.Http.StatusCode.Value))
                            incorrectStatusesCount[item.MetaData.Http.StatusCode.Value] = 0;
                        incorrectStatusesCount[item.MetaData.Http.StatusCode.Value]++;
                    }
                        
                }
        }
        
        // Some output items had no status
        var numberOfItemsWithNoStatus = itemsWithNoStatusPerOutput.Sum(output => output.Value);
        if (numberOfItemsWithNoStatus > 0)
            throw new ArgumentException(
                $"{numberOfItemsWithNoStatus} output items have no Http Status at all, check your" +
                " DataSaveType for all relevant outputs is configured to one that displays" +
                " MetaData and that all your outputs are Http protocol outputs." +
                "\n Each output and the number of items that have no status for it are as follows: " +
                string.Join(", ", itemsWithNoStatusPerOutput.ToList().Select(kv
                    => $"output {kv.Key} - {kv.Value} items")));
        
        // Some output items had an incorrect status
        var numberOfItemsWithIncorrectStatus = itemsWithIncorrectStatusPerOutput.Sum(output => output.Value);
        if (numberOfItemsWithIncorrectStatus > 0)
        {
            AssertionMessage =
                $"{numberOfItemsWithIncorrectStatus} output items dont have the expected Http Status code {Configuration.StatusCode!.Value}";
            AssertionTrace =
                $"The expected Http Status code is {Configuration.StatusCode!.Value} but " +
                "the following status codes were encountered: " +
                string.Join(", ", incorrectStatusesCount.ToList().Select(kv
                    => $"status code {kv.Key} - {kv.Value} times")) +
                ".\n Each output and the number of items that have an incorrect status in it are as follows: " +
                string.Join(", ", itemsWithIncorrectStatusPerOutput.ToList().Select(kv
                    => $"output {kv.Key} - {kv.Value} items"));
            return false;
        }
        
        // All output items had the correct status
        AssertionMessage = $"All configured outputs arrived with status {Configuration.StatusCode!.Value}";
        return true;
    }
    
}
