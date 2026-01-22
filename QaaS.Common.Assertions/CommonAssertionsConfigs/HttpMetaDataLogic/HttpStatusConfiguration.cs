using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.HttpMetaDataLogic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.HttpMetaDataLogic;

[Description("Checks that all configured output's http status code is equal to a certain number." +   
             " `DataSources`: Not used." +
             " `Session Support`: Only supports a single session assertion"),
 Display(Name = nameof(HttpStatus))]
public record HttpStatusConfiguration
{
    [Required, Range(100, 599), Description("The http status code all configured outputs should have")]
    public int? StatusCode { get; set; }
    
    [Required, MinLength(1), Description("The names of the outputs to check the status code of")] 
    public string[]? OutputNames { get; set; }
}