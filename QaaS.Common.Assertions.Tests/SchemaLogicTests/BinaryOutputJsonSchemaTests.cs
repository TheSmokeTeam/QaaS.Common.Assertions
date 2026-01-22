using System.Reflection;
using System.Text.Json.Nodes;
using NUnit.Framework;
using QaaS.Common.Assertions.SchemaLogic;
using QaaS.Common.Assertions.Tests.TestData.Binary;

namespace QaaS.Common.Assertions.Tests.SchemaLogicTests;

[TestFixture]
public class BinaryOutputJsonSchemaTests
{
    private static readonly MethodInfo DeserializeMethod =
        typeof(ObjectOutputJsonSchema).GetMethod("DeserializeOutputToJson", 
            BindingFlags.Static | BindingFlags.NonPublic)!;

    [Test]
    public void TestDeserializeOutputToJsonWithType_CallFunctionWithMessagePack_ShouldConvertMessagePackToCorrectJson()
    {
        // Arrange
        var testObject = new TestBinaryClass
        {
            IntegerProperty = 1,
            StringProperty = "REDA",
            DoubleProperty = 1.2
        };
        var assertion = new ObjectOutputJsonSchema();
        
        // Act
        var json = (JsonNode?)DeserializeMethod.Invoke(assertion, 
            new object?[]{testObject});
        
        // Assert
        Assert.AreEqual(3, json?.AsObject().Count);
    }
}