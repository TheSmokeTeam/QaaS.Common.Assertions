using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.Hermetic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;

[Description("Checks for IO hermetics by comparing the count of multiple outputs in a given session with certain" +
             " names to a given percentage of the count of multiple inputs in a given session with certain names." +
             " `DataSources`: Not used." +
             " `Session Support`: Supports multiple sessions assertion. CommunicationData objects with same names will be referenced both by InputNames or OutputNames"
 ), Display(Name = nameof(HermeticByInputOutputPercentage))]
public record HermeticByInputOutputPercentageConfiguration
{
    [Required, MinLength(1), Description("The names of the outputs to sum the counts of ")]
    public string[]? OutputNames { get; set; }

    [Required, MinLength(1),
     Description("The names of the inputs the sum of outputs should be a given percentage of the sum of")]
    public string[]? InputNames { get; set; }

    [Required, Range(0, Double.MaxValue),
     Description(
         "The percentage of the sum of inputs count the outputs count should be equal to for the assertion to pass")]
    public double? ExpectedPercentage { get; set; }

    [Description($"Whether the given inputs in {nameof(InputNames)} are actually another outputs list"),
     DefaultValue(false)]
    public bool InputsAreOutputs { get; set; } = false;

    [Description("Specifies the strategy the mathematical rounding method used for calculating the expected number of" +
                 " outputs (by taking a percentage of the sum of inputs) should use to round a number. Options: [ " +
                 "`AwayFromZero` - The strategy of rounding to the nearest number, and when a number is halfway between " +
                 "two others, it's rounded toward the nearest number that's away from zero / " +
                 "`ToEven` - The strategy of rounding to the nearest number," +
                 " and when a number is halfway between two others, it's rounded toward the nearest even number / " +
                 "`ToZero` - The strategy of directed rounding toward zero, with the result closest to and no " +
                 "greater in magnitude than the infinitely precise result / " +
                 "`ToNegativeInfinity` - The strategy of downwards-directed rounding, with the result closest to and" +
                 " no greater than the infinitely precise result / " +
                 "`ToPositiveInfinity` - The strategy of upwards-directed rounding, with the result closest to " +
                 "and no less than the infinitely precise result ]"),
     DefaultValue(MidpointRounding.AwayFromZero)]
    public MidpointRounding MidpointRounding { get; set; } = MidpointRounding.AwayFromZero;
}