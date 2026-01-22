using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.HttpMetaDataLogic;
using QaaS.Common.Assertions.Delay;

namespace QaaS.Common.Assertions.Tests.HttpMetaDataLogicTests.ConfigurationObjectsTests;

[TestFixture]
public class HttpStatusConfigurationTests
{
    [Test, 
     TestCase(0, false, 1), 
     TestCase(-100, false, 1),
     TestCase(99, false, 1), 
     TestCase(600, false, 1),
     TestCase(100, true, 0), 
     TestCase(599, true, 0)]
    public void TestValidateConfiguration_ConfigurationContainsHttpStatus_ShouldBeValidOrNotAccordingToIsValidParameter(int httpStatusCode, 
        bool isValid, int expectedNumberOfValidationResults)
    {
        // Arrange
        var configuration = new HttpStatusConfiguration
        {
            OutputNames = new []{"test"},
            StatusCode = httpStatusCode
        };
        
        // Act
        var validationResults = new List<ValidationResult>();
        var validationResult = Validator.TryValidateObject(configuration,
            new ValidationContext(configuration, serviceProvider: null, items: null),
             validationResults, validateAllProperties: true);
        
        // Assert
        Assert.AreEqual(isValid, validationResult);
        Assert.AreEqual(expectedNumberOfValidationResults, validationResults.Count);
    }
}