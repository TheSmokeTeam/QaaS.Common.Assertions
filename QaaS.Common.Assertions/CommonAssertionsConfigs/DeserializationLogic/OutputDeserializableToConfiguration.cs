using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.DeserializationLogic;
using QaaS.Framework.Serialization;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.DeserializationLogic;

[Description("Checks that all items in a configured output can be deserialized with a configured deserializer." +
" `DataSources`: Not used. `Session Support`: Only supports a single session assertion"),
 Display(Name = nameof(OutputDeserializableTo))]
public record OutputDeserializableToConfiguration
{
    [Required, Description("The names of the output who'se items should be deserializable")] 
    public string? OutputName { get; set; } 
    [Required, Description("The deserializer all output items should be deserializable with. " +
                           "Options are all available `QAAS.Base.Serialization` deserializers")] 
    public DeserializeConfig? Deserialize { get; set; }
}