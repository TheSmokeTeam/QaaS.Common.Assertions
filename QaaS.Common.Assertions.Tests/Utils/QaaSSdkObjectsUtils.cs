using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using QaaS.Common.Assertions.Tests.Mocks;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.Utils;

internal static class QaaSSdkObjectsUtils
{
    public static IEnumerable<SessionData> BuildSessionList(IEnumerable<JsonNode?> jsons, string outputName)
    {
        return new List<SessionData>
        {
            new()
            {
                Outputs = new List<CommunicationData<object>>
                {
                    new()
                    {
                        Name = outputName,
                        Data = jsons.Select(json => new DetailedData<object> { Body = json }).ToList()
                    }
                }
            }
        };
    }

    public static IEnumerable<DataSource> BuildDataSourceList(string fileName, string csvFileContent)
    {
        return new List<DataSource>
        {
            new()
            {
                Name = "DataSource:)",
                Generator = new MockGenerator(new List<Data<object>>
                {
                    new()
                    {
                        MetaData = new MetaData { Storage = new Storage { Key = fileName } },
                        Body = Encoding.UTF8.GetBytes(csvFileContent)
                    }
                })
            }
        };
    }
    
    public static IEnumerable<DataSource> BuildDataSourceList(object? content)
    {
        return new List<DataSource>
        {
            new()
            {
                Name = "DataSource",
                Generator = new MockGenerator(new List<Data<object>>
                {
                    new()
                    {
                        Body = content
                    },
                })
            }
        };
    }
}