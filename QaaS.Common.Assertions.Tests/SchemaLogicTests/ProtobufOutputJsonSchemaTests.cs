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
            Name = "REDA"
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
}