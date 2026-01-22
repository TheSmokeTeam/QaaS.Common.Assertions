using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;

namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

public class Base64ToHexFieldValidator(Base64ToHexFieldValidationConfig validationConfig) :
    BaseFieldValidator<Base64ToHexFieldValidationConfig>(validationConfig)
{
    public override bool Validate(object? fieldValue, object? expectedValue)
    {
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue), "Field value cannot be null");
        if (expectedValue == null)
            throw new ArgumentNullException(nameof(expectedValue), "Expected value cannot be null");
        
        return ConvertBase64ToHex(fieldValue) == expectedValue.ToString();
    }

    private static string ConvertBase64ToHex(object fieldValue)
    {
        return BitConverter.ToString(Convert.FromBase64String(
            fieldValue.ToString()!)).Replace("-", "");
    }
}