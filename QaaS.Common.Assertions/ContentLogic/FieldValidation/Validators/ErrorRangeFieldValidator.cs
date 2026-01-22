using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;

namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

public class ErrorRangeFieldValidator : BaseFieldValidator<ErrorRangeFieldValidatorConfig>
{
    public ErrorRangeFieldValidator(ErrorRangeFieldValidatorConfig validationConfig) : base(validationConfig)
    {
    }

    public override bool Validate(object? fieldValue, object? expectedValue)
    {
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue), "Field value cannot be null");
        if (expectedValue == null)
            throw new ArgumentNullException(nameof(expectedValue), "Expected value cannot be null");
        if (!double.TryParse(fieldValue.ToString(), out var val))
            throw new InvalidCastException(
                $"Could not cast the field's value from the output as double, value was: {fieldValue}");
        if (!double.TryParse(expectedValue.ToString(), out var expectedVal))
            throw new InvalidCastException(
                $"Could not cast the field's expected value from the expected results as double, value was: {fieldValue}");
        return Math.Abs(val - expectedVal) <= ValidationConfig.ErrorRange;
    }
}