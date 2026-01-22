using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Nodes;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Common.Assertions.ContentLogic;
using QaaS.Common.Assertions.Tests.Utils;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.ContentLogicTests.OutputContentByExpectedResultsTests;

[TestFixture]
public class AssertionOutputContentByExpectedCsvResultsTests
{
    private const string OutputName = "output-test";

    private static readonly JsonObject Json = new()
    {
        { "name", "REDA" },
        { "age", "20" },
        { "gender", "female" },
        {
            "address", new JsonObject()
            {
                { "city", "REDA" }
            }
        }
    };
    
    private static IEnumerable<TestCaseData> _singleSessionAssert = new[]
    {
        new TestCaseData(Json, "resultsKey", "NAME,AGE,CITY,GENDER\nREDA,21,REDA,girl",
                new Dictionary<string, FieldConfiguration>
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
                    }
                }, true,"100% match","")
            .SetName("ValidOutput_ShouldPass"),
        new TestCaseData(Json, "resultsKey", "NAME,AGE,CITY,GENDER\nREDA,21,REDA,girl",
                new Dictionary<string, FieldConfiguration>
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
                                    ErrorRange = 0.5
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
                    }
                }, false,"0% match","Invalid fields: AGE")
            .SetName("FieldOutOfErrorRange_ShouldFail"),
        new TestCaseData(Json, "resultsKey", "NAME,AGE,CITY,GENDER\nREDA,21,REDA,girl",
                new Dictionary<string, FieldConfiguration>
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
                                    ErrorRange = 5
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
                                    OverrideValue = "male"
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
                    }
                }, false,"0% match","Invalid fields: GENDER")
            .SetName("FieldNotMatchingOverrideValue_ShouldFail")
    };

    [Test, TestCaseSource(nameof(_singleSessionAssert))]
    public void TestAssertionResultsAsCsv_AssertsThatOutputMatchedResults_ShouldAssertTrueIfMatching(JsonNode json,
        string resultsMetaDataStorageKey, string resultsAsCsvContent,
        Dictionary<string, FieldConfiguration> mapping, bool shouldPass, string expectedMessage,string expectedTrace)
    {
        // Arrange
        var jsons = new List<JsonNode> { json };
        var sessionDataList = QaaSSdkObjectsUtils.BuildSessionList(jsons, OutputName).ToImmutableList();
        var externalDataList = QaaSSdkObjectsUtils.BuildDataSourceList(resultsMetaDataStorageKey, resultsAsCsvContent)
            .ToImmutableList();
        var assertion = new OutputContentByExpectedCsvResults()
        {
            Configuration = new OutputContentByExpectedResultsAsCsvConfiguration
            {
                OutputName = OutputName, ResultsMetaDataStorageKey = resultsMetaDataStorageKey,
                ColumnNameToFieldPathMap = mapping
            }
        };

        // Act
        var result = assertion.Assert(sessionDataList, externalDataList);

        // Assert
        Assert.AreEqual(shouldPass, result);
        Assert.That(assertion.AssertionMessage!.Contains(expectedMessage));
        Assert.That(assertion.AssertionTrace!.Contains(expectedTrace));
    }


    [Test, TestCase("No DataSource was provided for the assertion")]
    public void
        TestGetResultsDataSource_GetsTheDataSourceOfResultsFromDataSourcesList_ThrowsExceptionIfNoDataSourcesProvided(
            string expectedExceptionMessage)
    {
        // Arrange
        var sessionDataList = new List<SessionData>().ToImmutableList();
        var externalDataList = new List<DataSource>().ToImmutableList();
        var assertion = new OutputContentByExpectedCsvResults();

        // Act + Assert
        var ex = Assert.Throws<ArgumentException>(() => assertion.Assert(sessionDataList, externalDataList));
        if (ex != null)
            Assert.That(ex.Message.Contains(expectedExceptionMessage));
        else
            Assert.Fail("No exception was thrown");
    }


    [Test, TestCase("key1", "key2", "does not exist in the DataSource list")]
    public void
        TestLoadResultsContent_LoadsTheResultsContentFromDataSourcesListByTheKeyOfTheStorageMetadata_ThrowsExceptionIfNoDataSourcesHasThatKey(
            string actualFileName, string resultsMetaDataStorageKey,
            string expectedExceptionMessage)
    {
        // Arrange
        var sessionDataList = new List<SessionData>().ToImmutableList();
        var externalDataList = QaaSSdkObjectsUtils.BuildDataSourceList(actualFileName, "").ToImmutableList();
        var assertion = new OutputContentByExpectedCsvResults()
        {
            Configuration = new OutputContentByExpectedResultsAsCsvConfiguration
            {
                OutputName = OutputName, ResultsMetaDataStorageKey = resultsMetaDataStorageKey,
                ColumnNameToFieldPathMap = new Dictionary<string, FieldConfiguration>()
            }
        };

        // Act + Assert
        var ex = Assert.Throws<ArgumentException>(() => assertion.Assert(sessionDataList, externalDataList));
        Console.WriteLine(ex);
        if (ex != null)
            Assert.That(ex.Message.Contains(expectedExceptionMessage));
    }
}