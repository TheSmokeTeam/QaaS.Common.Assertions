using System.Collections.Immutable;
using System.Text;
using QaaS.Common.Assertions.CommonAssertionsConfigs.DeserializationLogic;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;
using QaaS.Framework.Serialization;

namespace QaaS.Common.Assertions.DeserializationLogic;

/// <summary>
/// Performs a logic test that checks if the items of a configured output can all be deserialized using a configured
/// deserializer
/// </summary>
/// <qaas-docs group="Contract validation" subgroup="Deserialization validation" />
public class OutputDeserializableTo: BaseAssertion<OutputDeserializableToConfiguration>
{
   
    /// <inheritdoc />
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var sessionData = sessionDataList.AsSingle();
        var output = sessionData.GetOutputByName(Configuration.OutputName!);
        var deserializer = DeserializerFactory.BuildDeserializer(Configuration.Deserialize!.Deserializer!.Value);
        
        var invalidOutputItemsCount = 0;
        var outputItemIndex = 0;
        var traceStringBuilder = new StringBuilder();
        foreach (var itemBody in output.Data.Select(item => item.CastObjectDetailedData<byte[]>()))
        {
            try
            {
                deserializer?.Deserialize(itemBody.Body,
                    Configuration.Deserialize.SpecificType != null
                        ? Configuration.Deserialize.SpecificType!.GetConfiguredType()
                        : null);
            }
            catch (Exception e)
            {
                traceStringBuilder.Append($"\nEncountered the following exception when trying to deserialize " +
                                          $" output item at index {outputItemIndex}:\n{e}\n");
                invalidOutputItemsCount++;
            }
            outputItemIndex++;
        }

        AssertionMessage = $"Out of {output.Data.Count} items in output {Configuration.OutputName!}, " +
                           $"{invalidOutputItemsCount} could not be deserialized with the {Configuration.Deserialize.Deserializer!}" +
                           " deserializer";
        AssertionTrace = traceStringBuilder.ToString();
        return invalidOutputItemsCount == 0;
    }
}
