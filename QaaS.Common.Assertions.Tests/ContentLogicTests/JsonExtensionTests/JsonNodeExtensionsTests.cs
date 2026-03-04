using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Path;
using NUnit.Framework;
using QaaS.Common.Assertions.ContentLogic.JsonExtensions;

namespace QaaS.Common.Assertions.Tests.ContentLogicTests.JsonExtensionTests;

[TestFixture]
public class JsonNodeExtensionsTests
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

    private static IEnumerable<TestCaseData> _validJsonPaths = new[]
    {
        new TestCaseData("$.gender", "female")
            .SetName("ValidPath"),
        new TestCaseData("$.address.city", "Boston")
            .SetName("ValidPath2"),
    };


    [Test, TestCase("$.blah"), TestCase("$.address.blah")]
    public void TestGetFieldValueByPath_TryToGetValue_ThrowsArgumentException(string fieldPath)
    {
        var ex = Assert.Throws<ArgumentException>(() => Json.GetFieldValueByPath(fieldPath));
        if (ex != null)
            Assert.That(ex.Message.Contains("Field not found for JSONPath given"));
        else
            Assert.Fail("No exception was thrown");
    }

    [Test, TestCase("hi", "Path must start with '$'"), TestCase("", "Input string is empty")]
    public void TestGetFieldValueByPath_TryToGetValue_ThrowsPathParseException(string fieldPath, string expectedExcMsg)
    {
        var ex = Assert.Throws<PathParseException>(() => Json.GetFieldValueByPath(fieldPath));
        if (ex != null)
            Assert.That(ex.Message.Contains(expectedExcMsg));
        else
            Assert.Fail("No exception was thrown");
    }

    [Test, TestCaseSource(nameof(_validJsonPaths))]
    public void TestGetFieldValueByPath_TryToGetValue_ReturnsTheExpectedValue(string fieldPath, string resultInJson)
    {
        var result = Json.GetFieldValueByPath(fieldPath)!.ToString();
        Assert.That(result! == resultInJson);
    }
}
