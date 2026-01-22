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

public class ObjectOutputJsonSchema : BaseAssertion<ObjectOutputJsonSchemaConfiguration>
{
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
        var jsonOutputIndex = 0;
        var traceStringBuilder = new StringBuilder();
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
          
            
            // Check if json item is valid according to at least one of the given schemas
            var isValid = false;
            var validationResults = new List<KeyValuePair<string, string>>();
            foreach (var jsonSchema in jsonSchemas)
            {
                var evaluationResults = jsonSchema.Value.Evaluate(deserializedOutput, new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List // Needs to be list so we dont have to recursively go over all sub fields in order to get all errors
                });
                if (evaluationResults.IsValid)
                {
                    isValid = true;
                    continue;
                }

                var evaluationStringMessages = new List<string>();
                AddEvaluationResultsToEvaluationStringMessagesAsString(evaluationResults, ref evaluationStringMessages);
                foreach (var nodeEvaluationResult in evaluationResults.Details)
                {
                    if (!nodeEvaluationResult.IsValid)
                        AddEvaluationResultsToEvaluationStringMessagesAsString(nodeEvaluationResult, ref evaluationStringMessages);
                }
                validationResults.Add(new KeyValuePair<string, string>(jsonSchema.Key,
                    string.Join("\n \t- " , evaluationStringMessages)));
            }
            if (!isValid)
            {
                var jsonSchemaFailureReasons = string.Join("\n - ",
                    validationResults.Select(pair => $"Schema: {pair.Key}, Validation Errors: \n \t- {pair.Value}")
                        .ToArray());
                traceStringBuilder.Append($"\nJson output item at index {jsonOutputIndex} failed validation with " +
                                          $"the following schemas: \n - {jsonSchemaFailureReasons}\n");
                invalidJsons++;
            }

            jsonOutputIndex++;
        }

        AssertionMessage = $"{invalidJsons} json items from output {Configuration.OutputName!} did not pass " +
                           $"any of the following given json schemas validation: " +
                           $"{string.Join("\n-", jsonSchemas.Select(pair => pair.Key).ToArray())}";
        AssertionTrace = traceStringBuilder.ToString();
        return invalidJsons <= 0;
    }

    /// <summary>
    /// Deserialized the object representation of the output to a JsonNode
    /// </summary>
    private static JsonNode? DeserializeOutputToJson(object? body)
    {
        if(body?.GetType() != typeof(string))
            body = JsonSerializer.Serialize(body);
        return JsonNode.Parse(body as string);
    }

    private static void AddEvaluationResultsToEvaluationStringMessagesAsString(EvaluationResults evaluationResults,
        ref List<string> evaluationStringMessages)
    {
        if (!evaluationResults.HasErrors || evaluationResults.Errors is null)
            return;
        var errorsString = string.Join(" | " ,evaluationResults.Errors.Select(pair =>
                $"Error Type: {pair.Key}, Error Message: {pair.Value}"));
        evaluationStringMessages.Add(
            $"Schema Path: '{evaluationResults.EvaluationPath}', Json Path: '{evaluationResults.InstanceLocation}'," +
            $" Errors: [{errorsString}]");
    }
}