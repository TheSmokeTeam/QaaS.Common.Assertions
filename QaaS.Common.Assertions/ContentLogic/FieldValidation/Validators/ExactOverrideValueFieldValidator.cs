using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;

namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

public class ExactOverrideValueFieldValidator: ExactValueFieldValidator<ExactOverrideValueFieldValidatorConfig> 
{
    public ExactOverrideValueFieldValidator(ExactOverrideValueFieldValidatorConfig validationConfig):base(validationConfig)
    { }

    public override bool Validate(object? fieldValue, object? expectedValue)
    {
        return base.Validate(fieldValue, ValidationConfig.OverrideValue);
    }
}