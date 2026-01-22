using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;

[Description("Validates a numeric field by error range.  " +
             "The error range is the maximal allowed difference between the field's value and the expected value, " +
             "so that the value will be considered as valid"),
 Display(Name = nameof(ErrorRangeFieldValidator))]
public record ErrorRangeFieldValidatorConfig
{
    [Required,
     Description("The error range to validate the field by")]
    public double? ErrorRange { get; set; }
}