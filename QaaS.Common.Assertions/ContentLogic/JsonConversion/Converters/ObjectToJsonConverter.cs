using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;

public class ObjectToJsonConverter : IJsonConverter
{
    public JsonNode Convert(object jsonConvertableObject)
    {
        var jsonString = JsonConvert.SerializeObject(jsonConvertableObject);
        return JsonNode.Parse(jsonString)!;
    }
}