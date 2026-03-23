using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using QaaS.Common.Assertions.CommonAssertionsConfigs.SchemaLogic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.SchemaLogic;

/// <summary>
/// Validates that each configured output item matches at least one JSON schema provided by the configured data sources.
/// </summary>
public class ObjectOutputJsonSchema : BaseAssertion<ObjectOutputJsonSchemaConfiguration>
{
    private sealed record OutputValidationSummary(
        int Index,
        IReadOnlyList<string> MatchingSchemas,
        IReadOnlyList<KeyValuePair<string, List<string>>> FailedSchemas)
    {
        public bool Passed => MatchingSchemas.Count > 0;
    }

    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        // Load all given json schemas
        var jsonSchemas = new List<KeyValuePair<string, JsonSchema>>();
        foreach (var dataSource in dataSourceList)
        {
            var dataItemIndex = 0;
            foreach (var dataItem in dataSource.Retrieve())
            {
                var schemaId = $"{dataSource.Name} - {dataItemIndex}";
                if (dataItem.Body is not byte[] data)
                    throw new ArgumentException($"Json schema in data source {dataSource.Name} at index {dataItemIndex} " +
                                                $"was not given as not null serialized item, could not load it!",
                        schemaId);
                
                jsonSchemas.Add(new KeyValuePair<string, JsonSchema>(
                    key: schemaId, value: JsonSerializer.Deserialize<JsonSchema>(data) ??
                                          throw new ArgumentException("JsonSchema cannot be null",
                                              schemaId)));
                dataItemIndex++;
            }
        }
        
        // Validate all output items
        var invalidJsons = 0;
        var validJsons = 0;
        var jsonOutputIndex = 0;
        var traceStringBuilder = new StringBuilder();
        var outputValidationSummaries = new List<OutputValidationSummary>();
        var sessionData = sessionDataList.AsSingle();
        foreach (var outputItem in sessionData.GetOutputByName(Configuration.OutputName!).Data)
        {
            JsonNode? deserializedOutput;
            if (outputItem.Body == null)
                throw new ArgumentException(
                    "One of the given output items is null and its schema cannot be checked, did you " +
                    "configure output items to contain body?");
            try
            {
                deserializedOutput = DeserializeOutputToJson(outputItem.Body);
            }
            catch (Exception e)
            {
                throw new ArgumentException("One of the given output items could not be deserialized to json",
                    innerException: e);
            }

            var deserializedOutputElement =
                JsonSerializer.Deserialize<JsonElement>(deserializedOutput?.ToJsonString() ?? "null");
          
            
            // Check if json item is valid according to at least one of the given schemas
            var matchingSchemas = new List<string>();
            var failedSchemas = new List<KeyValuePair<string, List<string>>>();
            foreach (var jsonSchema in jsonSchemas)
            {
                var evaluationResults = jsonSchema.Value.Evaluate(deserializedOutputElement, new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List // Needs to be list so we dont have to recursively go over all sub fields in order to get all errors
                });
                if (evaluationResults.IsValid)
                {
                    matchingSchemas.Add(jsonSchema.Key);
                    continue;
                }

                var evaluationStringMessages = GetEvaluationMessages(evaluationResults);
                failedSchemas.Add(new KeyValuePair<string, List<string>>(
                    jsonSchema.Key,
                    evaluationStringMessages));
            }

            var outputValidationSummary = new OutputValidationSummary(
                jsonOutputIndex,
                matchingSchemas,
                failedSchemas);
            outputValidationSummaries.Add(outputValidationSummary);

            if (!outputValidationSummary.Passed)
            {
                invalidJsons++;
                traceStringBuilder.Append(BuildDetailedTraceForOutput(outputValidationSummary));
                jsonOutputIndex++;
                continue;
            }

            validJsons++;
            traceStringBuilder.Append(BuildDetailedTraceForOutput(outputValidationSummary));
            jsonOutputIndex++;
        }

        var totalOutputs = outputValidationSummaries.Count;
        var overallResult = invalidJsons switch
        {
            0 => "ALL_PASS",
            _ when validJsons == 0 => "ALL_FAIL",
            _ => "PARTIAL_PASS"
        };
        AssertionMessage =
            $"JSON schema validation summary for output {Configuration.OutputName!}: overall result {overallResult}. " +
            $"Total items: {totalOutputs}. Passed: {validJsons}. Failed: {invalidJsons}. " +
            $"Provided schemas ({jsonSchemas.Count}): {FormatSchemaList(jsonSchemas.Select(pair => pair.Key))}. " +
            $"{BuildTopLevelFailureSummary(outputValidationSummaries.FirstOrDefault(summary => !summary.Passed))} " +
            $"Detailed per-item results are available in AssertionTrace.";
        AssertionTrace = traceStringBuilder.ToString();
        return invalidJsons <= 0;
    }

    /// <summary>
    /// Deserialized the object representation of the output to a JsonNode
    /// </summary>
    private static JsonNode? DeserializeOutputToJson(object? body)
    {
        var bodyAsJsonString = body is string bodyString
            ? bodyString
            : JsonSerializer.Serialize(body);
        return JsonNode.Parse(bodyAsJsonString);
    }

    private static List<string> GetEvaluationMessages(EvaluationResults evaluationResults)
    {
        var evaluationStringMessages = new List<string>();
        AddEvaluationResultsToEvaluationStringMessagesAsString(evaluationResults, evaluationStringMessages);
        return evaluationStringMessages.Distinct().ToList();
    }

    private static void AddEvaluationResultsToEvaluationStringMessagesAsString(
        EvaluationResults evaluationResults,
        List<string> evaluationStringMessages)
    {
        if (evaluationResults.Errors is null || evaluationResults.Errors.Count == 0)
        {
            foreach (var nestedEvaluationResult in evaluationResults.Details ?? [])
                AddEvaluationResultsToEvaluationStringMessagesAsString(nestedEvaluationResult, evaluationStringMessages);

            return;
        }

        var errorsString = string.Join(" | ", evaluationResults.Errors.Select(pair =>
            $"Error Type: {pair.Key}, Error Message: {pair.Value}"));
        evaluationStringMessages.Add(
            $"Json Path: '{NormalizeJsonPath(evaluationResults.InstanceLocation.ToString())}', " +
            $"Schema Path: '{NormalizeJsonPath(evaluationResults.EvaluationPath.ToString())}', Errors: [{errorsString}]");

        foreach (var nestedEvaluationResult in evaluationResults.Details ?? [])
            AddEvaluationResultsToEvaluationStringMessagesAsString(nestedEvaluationResult, evaluationStringMessages);
    }

    private static string GetFirstValidationLine(string validationResult)
    {
        return validationResult
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "Validation failed";
    }

    private static string NormalizeJsonPath(string? path)
    {
        return string.IsNullOrWhiteSpace(path) ? "$" : path;
    }

    private static string BuildTopLevelFailureSummary(OutputValidationSummary? firstFailedOutputValidationSummary)
    {
        if (firstFailedOutputValidationSummary is null)
            return "All output items matched at least one provided schema.";

        if (firstFailedOutputValidationSummary.FailedSchemas.Count == 0)
            return $"First failing item: index {firstFailedOutputValidationSummary.Index}. Failure reason: no schemas were provided.";

        var firstFailedSchema = firstFailedOutputValidationSummary.FailedSchemas.First();
        return $"First failing item: index {firstFailedOutputValidationSummary.Index}. " +
               $"Failure reason: {firstFailedSchema.Key} => {GetFirstValidationLine(firstFailedSchema.Value.FirstOrDefault() ?? "Validation failed")}.";
    }

    private static string BuildDetailedTraceForOutput(OutputValidationSummary outputValidationSummary)
    {
        var traceStringBuilder = new StringBuilder()
            .Append($"\nJson output item at index {outputValidationSummary.Index} ")
            .Append(outputValidationSummary.Passed ? "passed" : "failed")
            .AppendLine(" validation.")
            .Append("Matching schemas: ")
            .AppendLine(FormatSchemaList(outputValidationSummary.MatchingSchemas));

        if (outputValidationSummary.FailedSchemas.Count == 0)
        {
            traceStringBuilder.AppendLine(outputValidationSummary.Passed
                ? "Schemas that did not match: none"
                : "Schemas that did not match: no schemas were provided");
            return traceStringBuilder.ToString();
        }

        traceStringBuilder.AppendLine("Schemas that did not match:");
        foreach (var failedSchema in outputValidationSummary.FailedSchemas)
        {
            traceStringBuilder.AppendLine($" - Schema: {failedSchema.Key}, Validation Errors:");
            foreach (var validationMessage in failedSchema.Value)
                traceStringBuilder.AppendLine($" \t- {validationMessage}");
        }

        return traceStringBuilder.ToString();
    }

    private static string FormatSchemaList(IEnumerable<string> schemaNames)
    {
        var schemaNameList = schemaNames.ToList();
        return schemaNameList.Count == 0 ? "none" : string.Join(", ", schemaNameList);
    }
}
