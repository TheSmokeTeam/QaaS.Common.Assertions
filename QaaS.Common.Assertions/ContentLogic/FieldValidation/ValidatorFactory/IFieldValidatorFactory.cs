using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.ValidatorFactory;

public interface IFieldValidatorFactory
{
    public IFieldValidator GetFieldValidator(FieldValidationConfig validationConfig);
}