using System.Reflection;
using System.Text.Json.Nodes;
using NUnit.Framework;
using QaaS.Common.Assertions.SchemaLogic;
using TestNamespace;

namespace QaaS.Common.Assertions.Tests.SchemaLogicTests;

[TestFixture]
public class ProtobufOutputJsonSchemaTests
{
    private static readonly MethodInfo DeserializeMethod =
        typeof(ObjectOutputJsonSchema).GetMethod("DeserializeOutputToJson", 
            BindingFlags.Static | BindingFlags.NonPublic)!;
    
    [Test]
    public void TestDeserializeOutputToJsonWithType_CallFunctionWithMessagePack_ShouldConvertMessagePackToCorrectJson()
    {
        // Arrange
        var testObject = new TestObject
        {
            Age = 1,
            Id = 123,
            Name = "Alice"
        };
        var assertion = new ObjectOutputJsonSchema();
        
        // Act
        var json = (JsonNode?)DeserializeMethod.Invoke(assertion, 
            new object?[]{testObject});
        
        // Assert
        Assert.AreEqual(testObject.Age, json?["Age"]?.GetValue<int>());
        Assert.AreEqual(testObject.Id, json?["Id"]?.GetValue<int>());
        Assert.AreEqual(testObject.Name, json?["Name"]?.GetValue<string>());
    }

    [Test]
    public void TestDeserializeOutputToJsonWithType_CallFunctionWithJsonString_ShouldParseGivenJsonAsIs()
    {
        // Arrange
        const string jsonString = "{\"Age\":42,\"Id\":7,\"Name\":\"Bob\"}";
        var assertion = new ObjectOutputJsonSchema();

        // Act
        var json = (JsonNode?)DeserializeMethod.Invoke(assertion, new object?[] { jsonString });

        // Assert
        Assert.AreEqual(42, json?["Age"]?.GetValue<int>());
        Assert.AreEqual(7, json?["Id"]?.GetValue<int>());
        Assert.AreEqual("Bob", json?["Name"]?.GetValue<string>());
    }
}
