using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;

namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

public class ExactValueFieldValidator<TExactValueValidationConfig> : BaseFieldValidator<TExactValueValidationConfig>
    where TExactValueValidationConfig : ExactValueFieldValidatorConfig, new()
{
    public ExactValueFieldValidator(TExactValueValidationConfig exactValueValidationConfig) : base(
        exactValueValidationConfig)
    {
    }

    public override bool Validate(object? fieldValue, object? expectedValue)
    {
        return (fieldValue == null || expectedValue == null)
            ? fieldValue == null && expectedValue == null
            : fieldValue!.ToString()!.Equals(expectedValue.ToString());
    }
}