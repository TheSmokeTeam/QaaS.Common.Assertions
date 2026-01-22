using System.Collections.Immutable;
using System.Text.Json.Nodes;
using QaaS.Common.Assertions.CommonAssertionsConfigs.Hermetic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Hermetic;

public class
    ValidateHermeticMetricsByInputOutputPercentage : BaseAssertion<ValidateHermeticMetricsByInputOutputPercentageConfig>
{
    private const string NameProperty = "__name__", ValueProperty = "value", Metric = "metric";

    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var inputCount = Configuration.InputNames!.Sum(input => sessionDataList.Sum(sessionData =>
            sessionData.TryGetInputByName(input, out var data) ? data!.Data.Count : 0));
        var outputCount = Configuration.OutputNames!.Sum(output => sessionDataList.Sum(sessionData =>
            sessionData.TryGetOutputByName(output, out var data) ? data!.Data.Count : 0));

        var outputInputResultPercentage = (double)outputCount * 100 / inputCount;
        AssertionMessage =
            $"Sum of outputs {string.Join(", ", Configuration.OutputNames!)} count is {outputCount}\n" +
            $"Sum of the inputs {string.Join(", ", Configuration.InputNames!)} count is {inputCount}\n" +
            $"Hermetic result based on input to output percentage {outputInputResultPercentage}\n\n";

        var collectorData = sessionDataList.AsSingle().GetOutputByName(Configuration.MetricOutputSourceName!)
            .CastCommunicationData<JsonObject>().Data;
        var inputValue = GetLatestMetricAndPrintAssertion(Configuration.InputMetricName!, collectorData, "Input");
        var outputValue = GetLatestMetricAndPrintAssertion(Configuration.OutputMetricName!, collectorData, "Output");
        var processingValue =
            GetLatestMetricAndPrintAssertion(Configuration.ProcessMetricName, collectorData, "Process");
        var combineValue = GetLatestMetricAndPrintAssertion(Configuration.CombineMetricName, collectorData, "Combine");
        var filteredValue =
            GetLatestMetricAndPrintAssertion(Configuration.FilteredMetricName, collectorData, "Filtered");
        var splitValue = GetLatestMetricAndPrintAssertion(Configuration.SplitMetricName, collectorData, "Split");

        // Based on hermetics formula
        var metricsHermeticsResultPercentage = inputValue + splitValue != 0
            ? (outputValue + processingValue + combineValue + filteredValue) * 100 / (inputValue + splitValue)
            : 0;

        AssertionMessage += $"\nHermetics result based on metrics formula: {metricsHermeticsResultPercentage:F4}%\n";

        return Math.Abs(metricsHermeticsResultPercentage - outputInputResultPercentage) < Configuration.Tolerance;
    }

    private long GetLatestMetricAndPrintAssertion(string? metricName, IList<DetailedData<JsonObject>> metricsResults,
        string metricPurpose)
    {
        if (metricName == null) return 0;
        var metricResult = GetLatestMetricValueFromMetricsResults(metricName, metricsResults);
        AssertionMessage += $"{metricPurpose} metric ({metricName}): {metricResult}\n";
        return metricResult;
    }

    private long GetLatestMetricValueFromMetricsResults(string metricName,
        IList<DetailedData<JsonObject>> metricsResults)
    {
        var currentMetricResults = metricsResults.Where(metricResult =>
        {
            var metricResultName = JsonNode.Parse(metricResult.Body?[Metric]?.ToString()!)?[NameProperty]?.ToString();
            return metricResultName != null && metricResultName == metricName;
        }).ToList();
        if (currentMetricResults.Count == 0) return 0;
        var latestCurrentMetricResult = currentMetricResults.MaxBy(detailedData => detailedData.Timestamp);
        var metricValue = Convert.ToInt64(latestCurrentMetricResult?.Body?[ValueProperty]?.GetValue<string>() ??
                                          throw new InvalidOperationException(
                                              $"Metrics result invalid, can't extract metric value for metric {metricName} in output {Configuration.MetricOutputSourceName}"));
        return metricValue;
    }
}