using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QaaS.Common.Assertions.Hermetic;

namespace QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;

[Description(
    "Checks for metrics' hermetics by comparing the hermetics' percentage based on the `metric hermetics formula` "+
    "to the hermetics percentage based on the outputs' count to inputs' count percentage" +
    " `DataSources`: Not used." +
    " `Session Support`: Supports multiple sessions assertion. CommunicationData objects with same names will be referenced both by InputNames or OutputNames"
    ), Display(Name = nameof(ValidateHermeticMetricsByInputOutputPercentage))]

public record ValidateHermeticMetricsByInputOutputPercentageConfig() 
{
    [Required, MinLength(1), Description("The names of the outputs to sum the counts of ")]
    public string[]? OutputNames { get; set; }

    [Required, MinLength(1),
     Description("The names of the inputs the sum of outputs should be a given percentage of the sum of")]
    public string[]? InputNames { get; set; }
    
    [Required, Description("The name of the output to take the metrics from")]
    public string? MetricOutputSourceName { get; set; }

    [Description(
         "The tolerance of difference between metrics` hermetics percantage and outputs' count to inputs' count hermetics percentage"),
     DefaultValue(0.01)]
    public double Tolerance { get; set; } = 0.01;

    [Required, Description("The name of the input metric of the metric hemetrics formula")]
    public string? InputMetricName { get; set; }

    [Required, Description("The name of the output metric of the metric hemetrics formula")]
    public string? OutputMetricName { get; set; }

    [Description("The name of the process metric of the metric hemetrics formula")]
    public string? ProcessMetricName { get; set; }

    [Description("The name of the combine metric of the metric hemetrics formula")]
    public string? CombineMetricName { get; set; }

    [Description("The name of the filtered metric of the metric hemetrics formula")]
    public string? FilteredMetricName { get; set; }

    [Description("The name of the split metric of the metric hemetrics formula")]
    public string? SplitMetricName { get; set; }
};