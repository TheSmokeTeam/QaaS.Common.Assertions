using System.Text.Json.Nodes;

namespace QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;

public class JsonToJsonConverter : IJsonConverter
{
    public JsonNode Convert(object jsonConvertableObject) => (JsonNode)jsonConvertableObject;
}