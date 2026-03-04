using System;
using NUnit.Framework;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

namespace QaaS.Common.Assertions.Tests.ContentLogicTests.ValidatorsTests;

[TestFixture]
public class Base64ToHexFieldValidatorTests
{
    private static readonly Base64ToHexFieldValidator FieldValidator =
        new(new Base64ToHexFieldValidationConfig());
    
    [Test]
    public void TestValidateBase64ToHex_AssertThatTheFieldIsBeingConvertedCorrectly_TrueIfTheValidationWorked()
    {
        const string base64Value = "IVarzYmZFA==";
        const string hexValue = "2156ABCD899914";
        
        // Act
        var result = FieldValidator.Validate(base64Value, hexValue);
        
        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void TestValidateBase64ToHex_FieldValueAndExpectedValueDoNotMatch_ReturnsFalse()
    {
        var result = FieldValidator.Validate("cGFzc3dvcmQ=", "DEADBEEF");

        Assert.IsFalse(result);
    }

    [Test]
    public void TestValidateBase64ToHex_FieldValueIsNull_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => FieldValidator.Validate(null, "A1"));

        Assert.That(exception!.ParamName, Is.EqualTo("fieldValue"));
    }

    [Test]
    public void TestValidateBase64ToHex_ExpectedValueIsNull_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => FieldValidator.Validate("cGFzc3dvcmQ=", null));

        Assert.That(exception!.ParamName, Is.EqualTo("expectedValue"));
    }

    [Test]
    public void TestValidateBase64ToHex_FieldValueIsNotBase64_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => FieldValidator.Validate("not-base64", "A1"));
    }
}
