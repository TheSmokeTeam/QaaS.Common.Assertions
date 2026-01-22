
namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Delay;

/// <summary>
/// How to create a chunk's time
/// </summary>
public enum ChunkTimeOption
{
    /// <summary>
    /// Average of all times in chunk
    /// </summary>
    Average,
    /// <summary>
    /// First time in chunk
    /// </summary>
    First,
    /// <summary>
    /// Last time in chunk
    /// </summary>
    Last
}