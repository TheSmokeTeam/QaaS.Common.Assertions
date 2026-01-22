using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.ValidatorFactory;

public class FieldValidatorFactory : IFieldValidatorFactory
{
    public IFieldValidator GetFieldValidator(FieldValidationConfig validationConfig)
    {
        return validationConfig.Type switch
        {
            FieldValidationType.ExactValue => new ExactValueFieldValidator<ExactValueFieldValidatorConfig>(validationConfig.ExactValue),
            FieldValidationType.ExactOverrideValue => new ExactOverrideValueFieldValidator(validationConfig.ExactOverrideValue!),
            FieldValidationType.Base64ToHex => new Base64ToHexFieldValidator(validationConfig.Base64ToHex!),
            FieldValidationType.ErrorRange =>
                new ErrorRangeFieldValidator(validationConfig.ErrorRange!),
            _ => throw new NotSupportedException($"Field validation of type {validationConfig.Type} is not supported"),
        };
    }
}