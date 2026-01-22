using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;

namespace QaaS.Common.Assertions.ContentLogic.JsonConversion.ConverterFactory;

public class JsonConverterFactory : IJsonConverterFactory
{
    public IJsonConverter BuildJsonConverter(JsonConverterType converterType)
    {
        return converterType switch
        {
            JsonConverterType.Json => new JsonToJsonConverter(),
            JsonConverterType.Object => new ObjectToJsonConverter(),
            JsonConverterType.Xml => new XmlToJsonConverter(),
            _ => throw new NotSupportedException($"Json converter of type {converterType} is not supported"),
        };
    }
}