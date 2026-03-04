using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.JsonConversion.ConverterFactory;


namespace QaaS.Common.Assertions.Tests.ContentLogicTests.JsonConverterTests;

[TestFixture]
public class JsonConverterTests
{
    private static readonly JsonObject Json = new()
    {
        { "name", "Alice" },
        { "age", "20" },
        { "gender", "female" },
        {
            "address", new JsonObject()
            {
                { "city", "Boston" }
            }
        }
    };

    private record YamlObject
    {
        public string name { get; set; } = string.Empty;

        public string age { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;

        public Address address { get; set; } = new();
    }

    private record Address
    {
        public string city { get; set; } = string.Empty;
    }

    private static IEnumerable<TestCaseData> _convertableObjectsAndConverters = new[]
    {
        new TestCaseData(Json, JsonConverterType.Json, Json.DeepClone())
            .SetName("Json"),
        new TestCaseData(
                new XElement("map",
                    new XElement("name", "Alice"),
                    new XElement("age", "20"),
                    new XElement("gender", "female"),
                    new XElement("address",
                        new XElement("city", "Boston"))
                ), JsonConverterType.Xml, new JsonObject { { "map", Json.DeepClone() } })
            .SetName("Xml"),
        new TestCaseData(new YamlObject
                    { name = "Alice", age = "20", gender = "female", address = new Address { city = "Boston" } },
                JsonConverterType.Object, Json.DeepClone())
            .SetName("Yaml"),
    };

    private static readonly JsonConverterFactory Factory = new();

    [Test, TestCaseSource(nameof(_convertableObjectsAndConverters))]
    public void Test(object objectToConvert, JsonConverterType type, JsonNode expectedResult)
    {
        // Arrange
        var converter = Factory.BuildJsonConverter(type);

        // Act
        var result = converter.Convert(objectToConvert);

        // Assert
        Assert.That(JsonNode.DeepEquals(result, expectedResult));
    }
}
