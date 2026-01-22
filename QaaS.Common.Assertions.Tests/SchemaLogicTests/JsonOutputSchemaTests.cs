using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.SchemaLogic;
using QaaS.Common.Assertions.SchemaLogic;
using QaaS.Common.Assertions.Tests.Mocks;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.SchemaLogicTests;

[TestFixture]
public class JsonOutputSchemaTests
{
    private static readonly string ComplexValidJsonSchema = File.ReadAllText(Path.Join("TestData", "Json", "complexJsonSchema.json"));
    private static readonly string ComplexJson = File.ReadAllText(Path.Join("TestData", "Json", "complexValidJson.json"));
    private const string InvalidJsonSchema = @"{
        ""$schema"": ""http://json-schema.org/draft-07/schema#"",
        ""id"": ""http://JschemaBuddy.net"",
        ""type"": ""object"",
        ""properties"": {
            ""SomeNonExistentField"": {
                ""id"": ""http://JschemaBuddy.net/SomeNonExistentField"",
                ""type"": ""string""
            },
        ""SomeOtherInExistentField"": {
            ""id"": ""http://JschemaBuddy.net/SomeOtherInExistentField"",
        ""type"": ""integer""
        }
        },
        ""required"": [
        ""SomeNonExistentField"",
        ""SomeOtherInExistentField""
            ]
    }";

    private static IEnumerable<TestCaseData> _singleSessionAssertCaseSource = new[]
    {
        new TestCaseData(0, ComplexJson, Array.Empty<string>(), true).SetName("0OutputsNoJsonSchemas"),
        new TestCaseData(1, ComplexJson, Array.Empty<string>(), false).SetName("1OutputNoJsonSchemas"),
        new TestCaseData(5, ComplexJson, Array.Empty<string>(), false).SetName("MultipleOutputsNoJsonSchemas"),

        new TestCaseData(0, ComplexJson, new[] { ComplexValidJsonSchema }, true).SetName("0OutputsOneValidJsonSchema"),
        new TestCaseData(1, ComplexJson, new[] { ComplexValidJsonSchema }, true).SetName("1OutputOneValidJsonSchema"),
        new TestCaseData(5, ComplexJson, new[] { ComplexValidJsonSchema }, true).SetName("MultipleOutputsOneValidJsonSchema"),

        new TestCaseData(0, ComplexJson, new[] { InvalidJsonSchema }, true).SetName("0OutputsOneInvalidJsonSchema"),
        new TestCaseData(1, ComplexJson, new[] { InvalidJsonSchema }, false).SetName("1OutputOneInvalidJsonSchema"),
        new TestCaseData(5, ComplexJson, new[] { InvalidJsonSchema }, false).SetName("MultipleOutputsOneInvalidJsonSchema"),

        new TestCaseData(0, ComplexJson, new[] { ComplexValidJsonSchema, InvalidJsonSchema }, true).SetName("0OutputsTwoJsonSchemas1Valid1Invalid"),
        new TestCaseData(1, ComplexJson, new[] { ComplexValidJsonSchema, InvalidJsonSchema }, true).SetName("1OutputTwoJsonSchemas1Valid1Invalid"),
        new TestCaseData(5, ComplexJson, new[] { ComplexValidJsonSchema, InvalidJsonSchema }, true).SetName(
            "MultipleOutputsTwoJsonSchemas1Valid1Invalid"),
        
        new TestCaseData(2000, "{}", new[] { InvalidJsonSchema }, false).SetName("TonsOfSimpleOutputsOneInvalidJsonSchema"),
    };
    
    [Test, TestCaseSource(nameof(_singleSessionAssertCaseSource))]
    public void
        TestSingleSessionAssert_CallFunctionWithGivenNumbersOfOutputsAndSchemas_ShouldPassOrFailDependingOnParameters
        (int numberOfOutputs, string json, string[] schemas, bool shouldPass)
    {
        // Arrange
        const string outputName = "test";
        
        var outputs = new List<DetailedData<object>>();
        for(var outputNumber = 0; outputNumber < numberOfOutputs; outputNumber ++)
        {
            outputs.Add(new DetailedData<object>{Body = json});
        }
        var outputSource = new CommunicationData<object>
        {
            Name = outputName,
            Data = outputs
        };
        var configurations = new ObjectOutputJsonSchemaConfiguration
        {
            OutputName = outputName
        };
        var session = new SessionData
        {
            Name = "Id1",
            Outputs = new List<CommunicationData<object>>{outputSource}
        };
        var sessionList = new List<SessionData>
        {
            session
        }.ToImmutableList();
        var assertion = new ObjectOutputJsonSchema
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations
        };

        var schemasEnumerable = schemas.Select(schema => new Data<object> {Body = Encoding.UTF8.GetBytes(schema)});

        
        // Act
        var result = assertion.Assert(sessionList, new List<DataSource> { new()
        {
            Name = "ExternalDataA",
            Generator = new MockGenerator(schemasEnumerable)
        } }.ToImmutableList());
        
        // Assert
        Assert.AreEqual(shouldPass, result);
    }

    private static IEnumerable<TestCaseData> _testSingleSessionAssertionFailureDueToDataSourceCaseSource = new[]
    {
        new TestCaseData(new DataSource[]
        {
            new ()
            {
                Name = "TestSource",
                Generator = new MockGenerator(new Data<object>[]{new(){Body = new JObject()}})
            }
        }.ToImmutableList(), 
            "Json schema TestSource:invalidItem was not given as a not null serialized external data, could not load it!")
            .SetName("ExternalDataItemNotSerialized"),
        new TestCaseData(new DataSource[]
        {
            new ()
            {
                Name = "TestSource",
                Generator = new MockGenerator(new Data<object>[]{new(){Body = null}})
            }
        }.ToImmutableList(), 
            "Json schema TestSource:invalidItem was not given as a not null serialized external data, could not load it!")
            .SetName("ExternalDataItemNull"),
        new TestCaseData(new DataSource[]
        {
            new ()
            {
                Name = "TestSource",
                Generator = new MockGenerator(new Data<object>[]{new(){Body = new byte[]{1,1,2}}})
            }
        }.ToImmutableList(), 
                "Unexpected character encountered while parsing value: \u0001. Path '', line 0, position 0.")
            .SetName("ExternalDataItemNotValidJsonSchema")
    };
    
    [Test, TestCaseSource(nameof(_testSingleSessionAssertionFailureDueToDataSourceCaseSource))]
    public void
        TestSingleSessionAssertionFailureDueToDataSource_CallFunctionWithDataSourceContainingAnItemThatIsNotAJsonSchema_ShouldThrowAnException
        (ImmutableList<DataSource> invalidDataSources, string expectedExceptionMessage)
    {
        // Arrange
        var configurations = new ObjectOutputJsonSchemaConfiguration
        {
            OutputName = "test"
        };
        var assertion = new ObjectOutputJsonSchema
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations
        };
        
        // Act + Assert
        Assert.Throws<Exception>(() =>
        {
            try
            {
                assertion.Assert(new List<SessionData>().ToImmutableList(), invalidDataSources);
            }
            catch (Exception e)
            {
                Globals.Logger.LogInformation("{Exception}", e);
                throw new Exception(e.Message);
            }
        }, message: expectedExceptionMessage);
    }

    private static IEnumerable<TestCaseData> _testSingleSessionAssertionFailureDueToOutputCaseSource = new[]
    {
        new TestCaseData(new List<object>{null}).SetName("OneNullOutput"),
        new TestCaseData(new List<object>{null, null, null}).SetName("MultipleNullOutputs"),
        new TestCaseData(new List<object>{"REDA"}).SetName("SingleInvalidOutput"),
        new TestCaseData(new List<object>{"REDA", "REDA", "REDA"})
            .SetName("MultipleInvalidOutputs"),
    };
    
    [Test, TestCaseSource(nameof(_testSingleSessionAssertionFailureDueToOutputCaseSource))]
    public void
        TestSingleSessionAssertionFailureDueToOutput_CallFunctionWithOutputContainingAnItemThatCannotBeConvertedToAJson_ShouldThrowAnException
        (List<object> invalidOutputBodies)
    {
        // Arrange
        var configurations = new ObjectOutputJsonSchemaConfiguration
        {
            OutputName = "test"
        };
        var assertion = new ObjectOutputJsonSchema
        {
            Context = new Context { Logger = Globals.Logger },
            Configuration = configurations
        };
        
        // Act + Assert
        var exception = Assert.Throws<Exception>(() =>
        {
            try
            {
                assertion.Assert(new List<SessionData>
                {
                    new()
                    {
                        Outputs = new List<CommunicationData<object>>
                        {
                            new ()
                            {
                                Name = configurations.OutputName,
                                Data = invalidOutputBodies.Select(body => new DetailedData<object>{Body = body}).ToList()
                            }
                        }
                    }
                }.ToImmutableList(), new List<DataSource> {new(){Name = "test", Generator = 
                    new MockGenerator(new List<Data<object>>(){new()
                    {
                        Body = Encoding.UTF8.GetBytes(ComplexValidJsonSchema)
                    }})}}.ToImmutableList());
            }
            catch (Exception e)
            {
                Globals.Logger.LogInformation("{Exception}", e);
                throw new Exception(e.Message);
            }
        });
        Globals.Logger.LogInformation("Encountered exception is {exception}", exception);
    }
}