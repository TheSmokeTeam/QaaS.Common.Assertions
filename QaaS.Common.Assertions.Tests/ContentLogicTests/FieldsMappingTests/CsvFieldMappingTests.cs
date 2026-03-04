using System;
using System.Collections.Generic;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic;
using QaaS.Common.Assertions.Tests.ContentLogicTests.ContentLogicTestsUtils;


namespace QaaS.Common.Assertions.Tests.ContentLogicTests.FieldsMappingTests;

[TestFixture]
public class CsvFieldsMappingValidationTests
{
    private static IEnumerable<TestCaseData> _mappingAndCsvContent = new[]
    {
        new TestCaseData(new List<Dictionary<string, object?>>
                {
                    new()
                        { { "NAME", "Alice" }, { "AGE", "21" }, { "GENDER", "girl" }, { "CITY", "Boston" } }
                },
                new Dictionary<string, FieldConfiguration>
                {
                    {
                        "NAME", new FieldConfiguration()
                            { Path = "$.NAME", FieldValidationConfig = new FieldValidationConfig() }
                    },
                    {
                        "AGE", new FieldConfiguration()
                            { Path = "$.AGE", FieldValidationConfig = new FieldValidationConfig() }
                    },
                    {
                        "GENDER", new FieldConfiguration()
                            { Path = "$.GENDER", FieldValidationConfig = new FieldValidationConfig() }
                    },
                    {
                        "CITY", new FieldConfiguration()
                            { Path = "$.CITY", FieldValidationConfig = new FieldValidationConfig() }
                    }
                }, true, Array.Empty<string>())
            .SetName("AllColumnsInMappingAreValid"),
        new TestCaseData(new List<Dictionary<string, object?>>
            {
                new()
                    { { "NAME2", "Alice" }, { "AGE2", "21" }, { "GENDER", "girl" }, { "CITY", "Boston" } }
            },
            new Dictionary<string, FieldConfiguration>
            {
                {
                    "NAME",
                    new FieldConfiguration()
                        { Path = "$.NAME", FieldValidationConfig = new FieldValidationConfig() }
                },
                {
                    "AGE",
                    new FieldConfiguration() { Path = "$.AGE", FieldValidationConfig = new FieldValidationConfig() }
                },
                {
                    "GENDER",
                    new FieldConfiguration()
                        { Path = "$.GENDER", FieldValidationConfig = new FieldValidationConfig() }
                },
                {
                    "CITY",
                    new FieldConfiguration()
                        { Path = "$.CITY", FieldValidationConfig = new FieldValidationConfig() }
                }
            }, false, new[] { "NAME", "AGE" })
            .SetName("ColumnsNamesInvalidInMapping"),
        new TestCaseData(new List<Dictionary<string, object?>>
                {
                    new() { { "", "Unknown" } }
                },
                new Dictionary<string, FieldConfiguration>
                {
                    {
                        "NAME",
                        new FieldConfiguration()
                            { Path = "$.NAME", FieldValidationConfig = new FieldValidationConfig() }
                    },
                    {
                        "AGE",
                        new FieldConfiguration() { Path = "$.AGE", FieldValidationConfig = new FieldValidationConfig() }
                    },
                    {
                        "GENDER",
                        new FieldConfiguration()
                            { Path = "$.GENDER", FieldValidationConfig = new FieldValidationConfig() }
                    },
                    {
                        "CITY",
                        new FieldConfiguration()
                            { Path = "$.CITY", FieldValidationConfig = new FieldValidationConfig() }
                    }
                }, false, new[] { "NAME", "AGE", "GENDER", "CITY" })
            .SetName("NoColumnsInTheResults"),
    };

    [Test, TestCaseSource(nameof(_mappingAndCsvContent))]
    public void TestValidateFieldsMapping_ValidateIfOutputMatchExpectedResults_ShouldAssertValid(
        List<Dictionary<string, object?>> csvFileContent,
        Dictionary<string, FieldConfiguration> mapping, bool isValid, IEnumerable<string> expectedColumnsNotFound)
    {
        // Arrange
        var assertion = new OutputContentByExpectedCsvResults()
        {
            Configuration = new OutputContentByExpectedResultsAsCsvConfiguration
            {
                OutputName = "output",
                ResultsMetaDataStorageKey = "key",
                ColumnNameToFieldPathMap = mapping
            }
        };

        ReflectionUtils<OutputContentByExpectedResultsAsCsvConfiguration>.GetPropertyInfo("_expectedResults").SetValue(assertion, csvFileContent);

        // Act
        var parameters = new object[] { new List<string>() };
        var validationResult = ReflectionUtils<OutputContentByExpectedResultsAsCsvConfiguration>.GetMethodInfo("ValidateFieldsMapping")
            .Invoke(assertion, parameters);

        // Assert
        Assert.AreEqual(validationResult, isValid);
        CollectionAssert.AreEquivalent((List<string>)parameters[0], expectedColumnsNotFound);
    }
}
