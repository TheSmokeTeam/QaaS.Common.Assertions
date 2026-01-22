using System.Text.Json.Nodes;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;

public class XmlToJsonConverter : IJsonConverter
{
    public JsonNode Convert(object jsonConvertableObject)
    {
        var xmlDoc = (XContainer)jsonConvertableObject;
        var jsonString = JsonConvert.SerializeXNode(xmlDoc);
        return JsonNode.Parse(jsonString)!;
    }
}