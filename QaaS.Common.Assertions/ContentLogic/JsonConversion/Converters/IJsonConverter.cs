using System.Text.Json.Nodes;

namespace QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;

public interface IJsonConverter
{
    JsonNode Convert(object jsonConvertableObject);
}