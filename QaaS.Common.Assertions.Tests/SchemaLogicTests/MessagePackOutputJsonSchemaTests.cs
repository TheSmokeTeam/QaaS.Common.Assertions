using System.Reflection;
using System.Text.Json.Nodes;
using MessagePack;
using NUnit.Framework;
using QaaS.Common.Assertions.SchemaLogic;

namespace QaaS.Common.Assertions.Tests.SchemaLogicTests;

[TestFixture]
public class MessagePackOutputJsonSchemaTests
{
    [MessagePackObject]
    public record SerializableObject
    {
        [Key(0)]
        public int? Number { get; set; }
        
        [Key(1)]
        public string? String { get; set; }
    }

    private static readonly MethodInfo DeserializeMethod =
        typeof(ObjectOutputJsonSchema).GetMethod("DeserializeOutputToJson", 
            BindingFlags.Static | BindingFlags.NonPublic)!;
    
    [Test]
    public void TestDeserializeOutputToJson_CallFunctionWithMessagePack_ShouldConvertMessagePackToCorrectJson()
    {
        // Arrange
        var serializableObject = new SerializableObject
        {
            Number = 1,
            String = "dsjfods"
        };

        var assertion = new ObjectOutputJsonSchema();

        // Act
        var json = (JsonNode?)DeserializeMethod.Invoke(assertion, new object?[]{serializableObject});
        
        // Assert
        Assert.AreEqual(serializableObject?.Number, json?["Number"]?.GetValue<int>());
        Assert.AreEqual(serializableObject?.String, json?["String"]?.GetValue<string>());

    }
}