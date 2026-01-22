using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;

[Description("Validates a field by a value that overrides the expected value.  " +
             "The field is expected to match exactly to the override value"),
 Display(Name = nameof(ExactOverrideValueFieldValidator))]
public record ExactOverrideValueFieldValidatorConfig : ExactValueFieldValidatorConfig
{
    [Required, Description("The value that will override the expected value of the field")]
    public string? OverrideValue { get; set; }
}