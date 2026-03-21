using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.Delay;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;

[Description("Delay checks the time interval between the inputs and outputs is not bigger than a configured" +
             " number by subtracting the average timestamp of all inputs from the average timestamp of all the outputs. " +
             "`DataSources`: Not used. `Session Support`: Only supports a single session assertion"),
 Display(Name = nameof(DelayByAverage))]
public record DelayByAverageConfiguration
{
    [Required, Description("Name of the output end point to check the outputs of")]
    public string? OutputName { get; set; }
    [Required, Description("Name of the input end point to check the inputs of")]
    public string? InputName { get; set; }

    [Description($"Whether the given input in {nameof(InputName)} is actually another outputs list"),
     DefaultValue(false)]
    public bool InputsAreOutputs { get; set; } = false;

    [Required, Range(0, long.MaxValue), 
     Description("maximum delay in milliseconds allowed for average delay for assertion to pass")]
    public long? MaximumDelayMs { get; set; }
    
    [Range(0, long.MaxValue), Description("maximum negative delay buffer of average delay in milliseconds, " +
                                          "if value falls within the buffer it is still compared to maximum allowed delay"),
     DefaultValue(100)]
    public long? MaximumNegativeDelayBufferMs { get; set; } = 100;
}
