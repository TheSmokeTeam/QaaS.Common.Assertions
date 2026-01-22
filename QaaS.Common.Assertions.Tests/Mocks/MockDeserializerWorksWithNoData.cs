using System;
using QaaS.Framework.Serialization.Deserializers;

namespace QaaS.Common.Assertions.Tests.Mocks;

public class MockDeserializerWorksWithNoData: IDeserializer
{
    public object? Deserialize(byte[]? data, Type? deserializeType)
    {
        throw new Exception("Mock deserializer doesn't know how to deserialize any item");
    }
}