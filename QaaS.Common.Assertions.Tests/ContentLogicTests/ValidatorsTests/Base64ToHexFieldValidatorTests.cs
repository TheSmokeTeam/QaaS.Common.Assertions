using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

namespace QaaS.Common.Assertions.Tests.ContentLogicTests.ValidatorsTests;

[TestFixture]
public class Base64ToHexFieldValidatorTests
{
    
    [Test]
    public void TestValidateBase64ToHex_AssertThatTheFieldIsBeingConvertedCorrectly_TrueIfTheValidationWorked()
    {
        // Arrange
        var fieldValidatorConfig = new Base64ToHexFieldValidationConfig();
        var fieldValidator = new Base64ToHexFieldValidator(fieldValidatorConfig);
        const string base64Value = "IVarzYmZFA==";
        const string hexValue = "2156ABCD899914";
        
        // Act
        var result = fieldValidator.Validate(base64Value, hexValue);
        
        // Assert
        Assert.IsTrue(result);
    }
}