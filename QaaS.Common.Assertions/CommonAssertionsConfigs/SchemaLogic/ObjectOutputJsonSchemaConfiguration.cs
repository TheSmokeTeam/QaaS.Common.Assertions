using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.SchemaLogic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.SchemaLogic;

[Description("Checks that all items in a configured output are valid according to atleast one of" +
             " the json schemas provided from the configured DataSources." +
             " Expects the output items to be deserialized to any type of C# object that can be converted to Json." +
             " `DataSources`: Used for loading json schemas, all data sources are loaded as json schemas," +
             " expects to receive their items serialized." +
             " `Session Support`: Only supports a single session assertion." +
             " `Supported Json Drafts`: [ " +
             "`\"$schema\": \"http://json-schema.org/draft-06/schema#\"` / " +
             "`\"$schema\": \"http://json-schema.org/draft-07/schema#\"` / " +
             "`\"$schema\": \"https://json-schema.org/draft/2019-09/schema\"` / " +
             "`\"$schema\": \"https://json-schema.org/draft/2020-12/schema\"` / " +
             "`\"$schema\": \"https://json-schema.org/draft/next/schema\"` ]"
), Display(Name = nameof(ObjectOutputJsonSchema))]
public record ObjectOutputJsonSchemaConfiguration
{
    [Required, Description(
         "The names of the output who'se items should all be valid according to atleast one of the json schemas provided")] 
    public string? OutputName { get; set; }
}