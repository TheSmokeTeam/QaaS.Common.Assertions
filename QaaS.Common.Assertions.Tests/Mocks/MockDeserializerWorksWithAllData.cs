using System;
using QaaS.Framework.Serialization.Deserializers;

namespace QaaS.Common.Assertions.Tests.Mocks;

public class MockDeserializerWorksWithAllData: IDeserializer
{
    public object? Deserialize(byte[]? data, Type? deserializeType)
    {
        return data;
    }
}