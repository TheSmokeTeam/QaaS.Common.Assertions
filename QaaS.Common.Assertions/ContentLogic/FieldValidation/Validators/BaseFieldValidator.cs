namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

public abstract class BaseFieldValidator<TValidationConfiguration>
    (TValidationConfiguration validationConfig) : IFieldValidator 
{
    protected readonly TValidationConfiguration ValidationConfig = validationConfig;

    public abstract bool Validate(object? fieldValue, object? expectedValue);
}