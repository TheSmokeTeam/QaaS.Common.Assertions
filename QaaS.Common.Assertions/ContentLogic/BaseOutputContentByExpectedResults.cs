using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Nodes;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.ExpectedResultsHandler;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.ValidatorFactory;
using QaaS.Common.Assertions.ContentLogic.JsonConversion.ConverterFactory;
using QaaS.Common.Assertions.ContentLogic.JsonExtensions;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;
namespace QaaS.Common.Assertions.ContentLogic;

public abstract class BaseOutputContentByExpectedResults<TConfig> : BaseAssertion<TConfig>
    where TConfig : OutputContentByExpectedResultsConfiguration, new()
{
    private IList<Dictionary<string, object?>> _expectedResults { get; set; } = new List<Dictionary<string, object?>>();

    /// <summary>
    /// Gets an instance of IResultsHandler
    /// </summary>
    /// <returns> A new instance of IResultsHandler</returns>
    protected abstract IExpectedResultsHandler BuildExpectedResultsHandler();

    /// <summary>
    /// Gets an instance of IFieldValidatorFactory
    /// </summary>
    /// <returns>A new instance of IFieldValidatorFactory</returns>
    protected virtual IFieldValidatorFactory BuildFieldValidatorFactory() => new FieldValidatorFactory();

    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList,
        IImmutableList<DataSource> dataSourceList)
    {
        // Get the expected results
        LoadResultsContent(dataSourceList);

        // Get the output items
        var sessionData = sessionDataList.AsSingle();
        var outputs = sessionData.GetOutputByName(Configuration.OutputName!).Data;
        if (outputs.Count != _expectedResults.Count)
        {
            AssertionMessage =
                $"The amount of expected results and the amount of outputs from {Configuration.OutputName} did not match. " +
                $"\nGot {_expectedResults.Count} expected results and {outputs.Count} outputs.";
            return false;
        }

        if (_expectedResults.Count != 0)
        {
            // Validate that the columns' names in the configured mapping exists in the result items
            if (!ValidateFieldsMapping(out var columnsNotFound))
                throw new ArgumentException(
                    $"The columns: {string.Join(", ", columnsNotFound)} were not found in the provided results item {Configuration.ResultsMetaDataStorageKey ?? ""}");
        }

        // Validate all output items
        var fieldValidationFactory = BuildFieldValidatorFactory();
        var jsonConverter = new JsonConverterFactory().BuildJsonConverter(Configuration.JsonConverterType);
        int resultIndex = 0, invalidItems = 0, emptyItems = 0;
        var traceStringBuilder = new StringBuilder();
        foreach (var output in outputs)
        {
            if (output.Body == null)
            {
                traceStringBuilder.Append(
                    $"- Item in index {resultIndex} from output {Configuration.OutputName} is empty.\n");
                emptyItems++;
                resultIndex++;
                continue;
            }

            var body = jsonConverter.Convert(output.Body);
            if (CheckIsOutputValid(body, _expectedResults[resultIndex++], fieldValidationFactory,
                    out var invalidFields)) continue;
            traceStringBuilder.Append(
                $"- Item in index {resultIndex} from output {Configuration.OutputName} did not match the expected result. " +
                $"Invalid fields: {string.Join(", ", invalidFields)}).\n");
            invalidItems++;
        }

        // Assert
        var didPass = emptyItems == 0 && invalidItems == 0;
        var successPercentage =
            outputs.Count == 0 ? 0 : (outputs.Count - emptyItems - invalidItems) * 100 / outputs.Count;
        AssertionMessage = didPass
            ? $"{successPercentage}% match: All output ({outputs.Count} items) from {Configuration.OutputName!} matched the expected results, supplied by {Configuration.ResultsMetaDataStorageKey}"
            : $"{successPercentage}% match: {invalidItems + emptyItems} output items (out of {outputs.Count}) from {Configuration.OutputName!} did not match the expected results, supplied by {Configuration.ResultsMetaDataStorageKey}.\n{emptyItems} empty items, {invalidItems} invalid items.";
        AssertionTrace = traceStringBuilder.ToString();
        return didPass;
    }

    /// <summary>
    /// Gets list of DataSources and loads the results if found
    /// </summary>
    /// <param name="dataSourceList">List of supplied data sources</param>
    /// <exception cref="ArgumentException">Raised if results file does not exist in any of the provided data sources</exception>
    private void LoadResultsContent(IImmutableList<DataSource> dataSourceList)
    {
        var dataItems = GetResultsDataSource(dataSourceList).Retrieve();
        var resultsContent = GetResultsContentFromDataSource(dataItems);
        _expectedResults = BuildExpectedResultsHandler().DeserializeExpectedResults(resultsContent);
    }

    /// <summary>
    /// Gets the list of data sources and returns the data source by the configured name
    /// If no data source name was supplied in the configuration,
    /// assumes only one datasource was provided and uses it
    /// </summary>
    /// <param name="dataSources"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private DataSource GetResultsDataSource(IImmutableList<DataSource> dataSources)
    {
        if (dataSources.Count == 0)
            throw new ArgumentException(
                $"No DataSource was provided for the assertion, as it uses a results data source item for validating the received output");
        return Configuration.DataSourceName == null
            ? dataSources.AsSingle()
            : dataSources.GetDataSourceByName(Configuration.DataSourceName);
    }

    /// <summary>
    /// Get the results item data from a datasource (the datasource's retrieved data)
    /// If no ResultsMetaDataStorageKey was supplied in the configuration, takes the first item
    /// </summary>
    /// <param name="dataItems">Data items of a data source</param>
    /// <returns>The data item that matches the FileName if supplied. The first data item otherwise</returns>
    private IEnumerable<Data<object>> GetResultsContentFromDataSource(IEnumerable<Data<object>> dataItems)
    {
        if (Configuration.ResultsMetaDataStorageKey == null)
            return dataItems;
        var filteredDataItems = dataItems
            .Where(dataItem => dataItem.MetaData?.Storage?.Key == Configuration.ResultsMetaDataStorageKey)
            .ToList();
        if (!filteredDataItems.Any())
            throw new ArgumentException(
                $"The provided key of the storage metadata that contains the results {Configuration.ResultsMetaDataStorageKey} does not exist in the DataSource list",
                Configuration.ResultsMetaDataStorageKey);
        return filteredDataItems;
    }

    /// <summary>
    /// Validates if all the columns in the mapping appear in the results file's columns.
    /// </summary>
    /// <param name="columnsNotFound">List of all the columns that were not found</param>
    /// <returns>True if all column in the mapping appear in th results file</returns>
    private bool ValidateFieldsMapping(out IList<string> columnsNotFound)
    {
        columnsNotFound = Configuration.ColumnNameToFieldPathMap!.Keys
            .Where(column => !_expectedResults[0].ContainsKey(column)).ToList();
        return columnsNotFound.Count == 0;
    }

    /// <summary>
    /// Gets a json output and the expected result and validates it.
    /// </summary>
    /// <param name="output">The output json</param>
    /// <param name="expectedResults">The output's expected results</param>
    /// <param name="fieldValidationFactory">Field validation factory</param>
    /// <param name="invalidOutputFields">All the fields that were not valid</param>
    /// <returns>True if all fields are valid according to the expected result</returns>
    private bool CheckIsOutputValid(JsonNode output, IDictionary<string, object?> expectedResults,
        IFieldValidatorFactory fieldValidationFactory, out IList<string> invalidOutputFields)
    {
        invalidOutputFields = new List<string>();
        foreach (var fieldConfiguration in Configuration.ColumnNameToFieldPathMap!)
        {
            var valueInOutput =
                output.GetFieldValueByPath(fieldConfiguration.Value.Path ?? $"$.{fieldConfiguration.Key}");
            var expectedOutput = expectedResults[fieldConfiguration.Key];
            if (!fieldValidationFactory.GetFieldValidator(fieldConfiguration.Value.FieldValidationConfig!)
                    .Validate(valueInOutput, expectedOutput))
                invalidOutputFields.Add(fieldConfiguration.Key);
        }

        return invalidOutputFields.Count == 0;
    }
}
