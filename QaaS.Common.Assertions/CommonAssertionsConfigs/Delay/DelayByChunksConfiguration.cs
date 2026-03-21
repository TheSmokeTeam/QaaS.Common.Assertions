using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using QaaS.Common.Assertions.Delay;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;

[Description("Delay checks the time interval between the inputs and outputs is not bigger than a configured " +
"number by subtracting the timestamp of input chunks of a configured size from a timestamp of output" +
    " chunks of a configured size, The timestamp of the chunks is calculated differently depending on the configuration. " +
    "takes the chunks in ascending order (which will be the send/arrival order) from the configured input/output." +
    " `Warning`: This delay test only works on synchronous applications! " +
    "`DataSources`: Not used. `Session Support`: Only supports a single session assertion"),
 Display(Name = nameof(DelayByChunks))]
public record DelayByChunksConfiguration
{
    [Required, Description("The information about the output required for the assertion ")]
    public Chunk? Output { get; set; }
    [Required, Description("The information about the input required for the assertion")]
    public Chunk? Input { get; set; }

    [Description($"Whether the given input in {nameof(Input)} is actually another outputs list"),
     DefaultValue(false)]
    public bool InputsAreOutputs { get; set; } = false;

    [Required, Range(0, long.MaxValue), Description(
         "maximum delay in milliseconds allowed for a chunk's delay for the chunk to be considered as arrived on time")]
    public long? MaximumDelayMs { get; set; }

    [Range(0, long.MaxValue), Description("maximum negative delay buffer of a chunk's delay in milliseconds, if value" +
                                          " falls within the buffer it is still compared to maximum allowed delay"),
     DefaultValue(100)]
    public long? MaximumNegativeDelayBufferMs { get; set; } = 100;
}
