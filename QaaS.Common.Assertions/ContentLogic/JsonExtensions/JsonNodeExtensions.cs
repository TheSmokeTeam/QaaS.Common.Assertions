using System.Text.Json.Nodes;
using Json.Path;

namespace QaaS.Common.Assertions.ContentLogic.JsonExtensions;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Get a value in a JsonNode using a json path.
    /// </summary>
    /// <param name="jsonNode">The json node to get the value from</param>
    /// <param name="jsonFieldPath">The json path to the value</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if no matches are found for the json path.</exception>
    public static object? GetFieldValueByPath(this JsonNode jsonNode, string jsonFieldPath)
    {
        var jsonPathObject = JsonPath.Parse(jsonFieldPath);
        var jsonPathMatch = jsonPathObject.Evaluate(jsonNode).Matches!.FirstOrDefault() ??
                            throw new ArgumentException($"Field not found for JSONPath given: {jsonFieldPath}");
        return jsonPathMatch.Value;
    }
}