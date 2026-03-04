using System;
using System.Collections.Generic;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.JsonConversion.ConverterFactory;
using QaaS.Common.Assertions.ContentLogic.JsonConversion.Converters;


namespace QaaS.Common.Assertions.Tests.ContentLogicTests.JsonConverterTests;

[TestFixture]
public class JsonConverterFactoryTests
{
    private static IEnumerable<TestCaseData> _converterTypes = new[]
    {
        new TestCaseData(JsonConverterType.Json, typeof(JsonToJsonConverter))
            .SetName("Json"),
        new TestCaseData(JsonConverterType.Xml, typeof(XmlToJsonConverter))
            .SetName("Xml"),
        new TestCaseData(JsonConverterType.Object, typeof(ObjectToJsonConverter))
            .SetName("Yaml"),
    };

    private static readonly JsonConverterFactory Factory = new();

    [Test, TestCaseSource(nameof(_converterTypes))]
    public void Test(JsonConverterType type, Type expectedConverterType)
    {
        // Arrange + Act
        var converter = Factory.BuildJsonConverter(type);

        // Assert
        Assert.That(converter.GetType(), Is.EqualTo(expectedConverterType));
    }

    [Test]
    public void TestBuildJsonConverter_WhenTypeIsUnsupported_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            Factory.BuildJsonConverter((JsonConverterType)1000));
    }
}
