using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.Hermetic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;

[Description("Checks for IO hermetics by comparing the count of multiple outputs in a " +
             "given session with certain names to a given expected count." +
             " `DataSources`: Not used. " +
             "`Session Support`: Supports multiple sessions assertion. CommunicationData objects with same names will be referenced both by InputNames or OutputNames"),
 Display(Name = nameof(HermeticByExpectedOutputCount))]
public record HermeticByExpectedOutputCountConfiguration
{
    [Required, MinLength(1), Description(
         "The names of the outputs to sum the counts of and then compare them to the expected count field in the given session")] 
    public string[]? OutputNames { get; set; }
    [Required, Range(0, int.MaxValue), 
     Description("The expected count of the items in the given output name in the given session")] 
    public int? ExpectedCount { get; set; }
}