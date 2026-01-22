using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.Hermetic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;

[Description("Checks for IO hermetics by comparing the count of multiple outputs in a " +
             "given session with certain names to be between a given expected maximum limit and minimum limit." +
             " `DataSources`: Not used. " +
             "`Session Support`: Supports multiple sessions assertion. CommunicationData objects with same names will be referenced both by InputNames or OutputNames"),
 Display(Name = nameof(HermeticByExpectedOutputCountInRange))]
public record HermeticByExpectedOutputCountInRangeConfiguration
{
    [Required, MinLength(1), Description(
         "The names of the outputs to sum the counts of and then compare them to the expected count field in the given session")] 
    public string[]? OutputNames { get; set; }
    
    [Required, Range(0, int.MaxValue), 
     Description("The expected minimum count of the items in the given output names in the given session")] 
    public int? ExpectedMinimumCount { get; set; }
    
    [Required, Range(0, int.MaxValue), 
     Description("The expected maximum count of the items in the given output names in the given session")] 
    public int? ExpectedMaximumCount { get; set; }
}