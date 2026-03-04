using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Common.Assertions.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.ValidatorFactory;
using QaaS.Common.Assertions.Tests.ContentLogicTests.ContentLogicTestsUtils;


namespace QaaS.Common.Assertions.Tests.ContentLogicTests.ValidatorsTests;

[TestFixture]
public class OutputValidationTests
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
        },
        { "password", "cGFzc3dvcmQ=" }
    };

    private static readonly Dictionary<string, FieldConfiguration> Mapping = new()
    {
        {
            "NAME", new FieldConfiguration
            {
                Path = "$.name",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue
                }
            }
        },
        {
            "AGE", new FieldConfiguration
            {
                Path = "$.age",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ErrorRange,
                    ErrorRange = new ErrorRangeFieldValidatorConfig
                    {
                        ErrorRange = 2
                    },
                }
            }
        },
        {
            "GENDER", new FieldConfiguration
            {
                Path = "$.gender",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactOverrideValue,
                    ExactOverrideValue = new ExactOverrideValueFieldValidatorConfig()
                    {
                        OverrideValue = "female"
                    },
                }
            }
        },
        {
            "CITY", new FieldConfiguration
            {
                Path = "$.address.city",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue,
                }
            }
        },
        {
            "PASSWORD", new FieldConfiguration
            {
                Path = "$.password",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.Base64ToHex
                }
            }

        }
    };
    
    private static readonly Dictionary<string, FieldConfiguration> ValidNoPathsMapping = new()
    {
        {
            "name", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue
                }
            }
        },
        {
            "age", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ErrorRange,
                    ErrorRange = new ErrorRangeFieldValidatorConfig
                    {
                        ErrorRange = 2
                    },
                }
            }
        },
        {
            "gender", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactOverrideValue,
                    ExactOverrideValue = new ExactOverrideValueFieldValidatorConfig()
                    {
                        OverrideValue = "female"
                    },
                }
            }
        }
    };

    private static IEnumerable<TestCaseData> _resultsAndMapping = new[]
    {
        new TestCaseData(Json,
                new Dictionary<string, object>
                {
                    { "NAME", "Alice" }, { "AGE", "21" }, { "GENDER", "girl" }, { "CITY", "Boston" },
                    { "PASSWORD", "70617373776F7264" }
                }, Mapping,
                true, new List<string>())
            .SetName("ValidOutput"),
        new TestCaseData(Json,
                new Dictionary<string, object>
                {
                    { "NAME", "Alice" }, { "AGE", "23" }, { "GENDER", "girl" }, { "CITY", "Boston" },
                    { "PASSWORD", "DEADBEEF" }
                }, Mapping,
                false, new List<string> { "AGE", "PASSWORD" })
            .SetName("InvalidFieldInOutput"),
        new TestCaseData(Json,
                new Dictionary<string, object>
                    { { "name", "Alice" }, { "age", "21" }, { "gender", "girl" } }, ValidNoPathsMapping,
                true, new List<string>())
            .SetName("NoPathsProvidedInMapping_Valid")
    };

    [Test, TestCaseSource(nameof(_resultsAndMapping))]
    public void TestCheckIsOutputValid_ChecksIfOutputIsValidAccordingToMappingAndExpectedResults_AssertValid(
        JsonNode output, Dictionary<string, object> expectedResult,
        Dictionary<string, FieldConfiguration> fieldsMapping, bool isValid, List<string> expectedColumnsNotFound)
    {
        // Arrange
        var assertion = new OutputContentByExpectedCsvResults()
        {
            Configuration = new OutputContentByExpectedResultsAsCsvConfiguration()
                { OutputName = "output", ResultsMetaDataStorageKey = "key", ColumnNameToFieldPathMap = fieldsMapping }
        };

        // Act
        var parameters = new object[] { output, expectedResult, new FieldValidatorFactory(), new List<string>() };
        var validationResult = ReflectionUtils<OutputContentByExpectedResultsAsCsvConfiguration>.GetMethodInfo("CheckIsOutputValid").Invoke(assertion, parameters);

        // Assert
        Assert.That(((bool)validationResult!).Equals(isValid));
        CollectionAssert.AreEquivalent((List<string>)parameters[3], expectedColumnsNotFound);
    }


    private static readonly Dictionary<string, FieldConfiguration> InvalidMapping = new()
    {
        {
            "name", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue
                }
            }
        },
        {
            "age", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ErrorRange,
                    ErrorRange = new ErrorRangeFieldValidatorConfig
                    {
                        ErrorRange = 2
                    },
                }
            }
        },
        {
            "gender", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactOverrideValue,
                    ExactOverrideValue = new ExactOverrideValueFieldValidatorConfig()
                    {
                        OverrideValue = "female"
                    },
                }
            }
        },
        {
            "city", new FieldConfiguration
            {
                Path = "$.country",
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue,
                }
            }
        }
    };

    private static readonly Dictionary<string, FieldConfiguration> InvalidNoPathsMapping = new()
    {
        {
            "name", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue
                }
            }
        },
        {
            "age", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ErrorRange,
                    ErrorRange = new ErrorRangeFieldValidatorConfig
                    {
                        ErrorRange = 2
                    },
                }
            }
        },
        {
            "gender", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactOverrideValue,
                    ExactOverrideValue = new ExactOverrideValueFieldValidatorConfig()
                    {
                        OverrideValue = "female"
                    },
                }
            }
        },
        {
            "city", new FieldConfiguration
            {
                FieldValidationConfig = new FieldValidationConfig
                {
                    Type = FieldValidationType.ExactValue,
                }
            }
        }
    };

    private static IEnumerable<TestCaseData> _resultsAndMappingWherePathNotExist = new[]
    {
        new TestCaseData(Json,
                new Dictionary<string, object>
                    { { "name", "Alice" }, { "age", "21" }, { "gender", "girl" }, { "city", "Boston" } },
                InvalidNoPathsMapping,
                false, "Field not found for JSONPath given: $.city")
            .SetName("NoPathsProvidedInMapping"),
        new TestCaseData(Json,
                new Dictionary<string, object>
                    { { "name", "Alice" }, { "age", "21" }, { "gender", "girl" }, { "city", "Boston" } },
                InvalidMapping,
                false, "Field not found for JSONPath given: $.country")
            .SetName("PathProvidedInMappingButDontExistInJson")
    };

    [Test, TestCaseSource(nameof(_resultsAndMappingWherePathNotExist))]
    public void
        TestCheckIsOutputValid_ChecksIfOutputIsValidAccordingToMappingAndExpectedResults_ThrowsExceptionCausePathNotExists(
            JsonNode output, Dictionary<string, object> expectedResult,
            Dictionary<string, FieldConfiguration> fieldsMapping, bool isValid, string expectedExceptionMessage)
    {
        // Arrange
        var assertion = new OutputContentByExpectedCsvResults()
        {
            Configuration = new OutputContentByExpectedResultsAsCsvConfiguration
                { OutputName = "output", ResultsMetaDataStorageKey = "key", ColumnNameToFieldPathMap = fieldsMapping }
        };
        var parameters = new object[] { output, expectedResult, new FieldValidatorFactory(), new List<string>() };


        // Act + Arrange
        var ex = Assert.Throws<TargetInvocationException>(() =>
            ReflectionUtils<OutputContentByExpectedResultsAsCsvConfiguration>.GetMethodInfo("CheckIsOutputValid").Invoke(assertion, parameters));
        if (ex!.InnerException != null)
        {
            Assert.That(ex.InnerException.GetType() == typeof(ArgumentException));
            Assert.That(ex.InnerException.Message.Contains(expectedExceptionMessage));
        }

        else
            Assert.Fail("No exception was thrown");
    }
}
