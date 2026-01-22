using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;

namespace QaaS.Common.Assertions.ContentLogic.JsonConversion.ConverterFactory;

public interface IJsonConverterFactory
{
    IJsonConverter BuildJsonConverter(JsonConverterType type);
}