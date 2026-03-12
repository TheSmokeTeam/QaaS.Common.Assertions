using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic.FieldsValidationConfig;
using QaaS.Framework.Configurations.CustomValidationAttributes;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;

/// <summary>
/// Checks for all json items in a configured output that their content is valid according to a results item.
/// The results file contains the expected value of fields in each json item and is provided from the configured DataSources.
/// Expects the output items to be deserialized to any type of C# object that can be converted to Json.
/// `DataSources`: Used for loading the results item.
/// `Session Support`: Only supports a single session assertion.
/// </summary>
public record OutputContentByExpectedResultsConfiguration
{
    [Required, Description("The name of the output to validate its items' content")]
    public string? OutputName { get; set; }

    [Description(
        "The storage key of the dataSource item of the results. If no key was provided, takes the first item of from the DataSource")]
    public string? ResultsMetaDataStorageKey { get; set; }

    [Description(
        "The name of the dataSource of the results item. If no name was provided, takes the first DataSource supplied")]
    public string? DataSourceName { get; set; }

    [Required, Description("The mapping for each column's name in the outputs results and its matchig field's path " +
                           "in the output json.")]
    public Dictionary<string, FieldConfiguration>? ColumnNameToFieldPathMap { get; set; }

    [Description("The type of json converter to use for converting the output items")]
    [DefaultValue(JsonConverterType.Json)]
    public JsonConverterType JsonConverterType { get; set; }

    [Description("If true, rows can match expected results in any order instead of by their index")]
    [DefaultValue(false)]
    public bool CompareRowsNotInOrder { get; set; }
}

public record FieldConfiguration
{
    [Required, Description("The path to the field in the json output")]
    public string? Path { get; set; }

    [Description("The configuration for the field's specific type of validation")]
    public FieldValidationConfig FieldValidationConfig { get; set; } = new();
}

public record FieldValidationConfig
{
    [Description("The type of validation the field will be validated according to")]
    [DefaultValue(FieldValidationType.ExactValue)]
    public FieldValidationType Type { get; set; } = FieldValidationType.ExactValue;

    [RequiredIfAny(nameof(Type), FieldValidationType.ExactValue),
     Description("Validates field by the exact value from the expected results")]
    public ExactValueFieldValidatorConfig ExactValue { get; set; } = new();
    
    [RequiredIfAny(nameof(Type), FieldValidationType.ErrorRange),
     Description("Validates field by an error range of the expected value")]
    public ErrorRangeFieldValidatorConfig? ErrorRange { get; set; }

    [RequiredIfAny(nameof(Type), FieldValidationType.ExactOverrideValue),
     Description("Validates field by a value that overrides the expected value")]
    public ExactOverrideValueFieldValidatorConfig? ExactOverrideValue { get; set; }

    [RequiredIfAny(nameof(Type), FieldValidationType.Base64ToHex),
     Description("Validates field by converting the value from base64 to hex")]
    public Base64ToHexFieldValidationConfig? Base64ToHex { get; set; }
}
