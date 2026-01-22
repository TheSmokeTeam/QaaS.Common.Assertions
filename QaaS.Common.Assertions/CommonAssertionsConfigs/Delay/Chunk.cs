using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;

public record Chunk
{
    [Required, Description("Name of the end point the chunk belongs to")]
    public string? Name { get; set; }
    [Required, Range(0, int.MaxValue), Description("The end point items chunk size")]
    public int? ChunkSize { get; set; }
    [Description("How to calculate the time of a chunk. Options: [ " +
                 "`Average` - Calculates the chunks time by taking the average of all times in the chunk / " +
                 "`First` - Calculates the chunks time by taking the time of the first item in the chunk / " +
                 "`Last` - Calculates the chunks time by taking the time of the last item in the chunk ]"),
     DefaultValue(ChunkTimeOption.Average)]
    public ChunkTimeOption ChunkTimeOption { get; set; } = ChunkTimeOption.Average;
}