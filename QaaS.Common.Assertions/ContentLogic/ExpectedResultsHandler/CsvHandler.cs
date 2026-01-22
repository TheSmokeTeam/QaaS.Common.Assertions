using System.Text;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Session.DataObjects;

namespace QaaS.Common.Assertions.ContentLogic.ExpectedResultsHandler;

public class CsvHandler : IExpectedResultsHandler
{
    /// <summary>
    /// Deserializes a csv file (only one file) which represented as a byte array into a list of dictionaries.
    /// The key is the header of the column and the values are the values in each line. 
    /// </summary>
    /// <param name="expectedResults"> The csv to deserialize </param>
    /// <returns> A list of dictionaries which represents the csv </returns>
    public IList<Dictionary<string, object?>> DeserializeExpectedResults(IEnumerable<Data<object>> expectedResults)
    {
        var bytes = ExtractCsvFile(expectedResults);

        using var stringReader = new StringReader(Encoding.UTF8.GetString(bytes));
        var results = new List<Dictionary<string, object?>>();

        // Read first line of the csv file
        var headerLine = stringReader.ReadLine();
        if (headerLine == null)
            return results;
        var columns = headerLine.Split(',');

        // Read the content lines of the csv file to the dictionaries list
        while (stringReader.ReadLine() is { } line)
        {
            var dict = new Dictionary<string, object?>();
            var values = line.Split(',');
            for (var i = 0; i < values.Length; i++)
                dict[columns[i]] = values[i];
            results.Add(dict);
        }

        return results;
    }

    /// <summary>
    /// Extracts the csv file from the expectedResults input. The expected results should contain only one item.
    /// </summary>
    /// <param name="expectedResults"> Iterable object that contains the expected results </param>
    /// <returns> The extracted file as byte array  </returns>
    /// <exception cref="ArgumentException"> If the extracted item is not byte array
    /// (csv file represented like that) raise an exception </exception>
    private static byte[] ExtractCsvFile(IEnumerable<Data<object>> expectedResults)
    {
        var extractedFile = expectedResults.AsSingle();
        if (extractedFile.Body is not byte[] bytes)
            throw new ArgumentException(
                $"The expected results csv file has to be serialized, and be represented as a byte array.",
                extractedFile.MetaData?.Storage?.Key);
        return bytes;
    }
}