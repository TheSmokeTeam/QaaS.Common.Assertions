using QaaS.Common.Assertions.DeserializationLogic;
using QaaS.Framework.Serialization.Deserializers;
using QaaS.Framework.Serialization.Serializers;

namespace QaaS.Common.Assertions.Tests.Mocks;

/// <summary>
/// Mock used to inject IByNameObjectCreator into the assertion `OutputDeserializableTo`
/// </summary>
public class OutputDeserializableToMock: OutputDeserializableTo
{
    private readonly IDeserializer deserializer;
    
    public OutputDeserializableToMock(IDeserializer serializer)
    {
        deserializer = serializer;
    }
    
}