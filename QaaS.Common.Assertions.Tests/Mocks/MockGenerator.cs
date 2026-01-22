using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Generator;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace QaaS.Common.Assertions.Tests.Mocks;

public class MockGenerator(IEnumerable<Data<object>> dataToGenerate) : IGenerator
{
    public List<ValidationResult>? LoadAndValidateConfiguration(IConfiguration configuration) => new();
    
    public Context Context { get; set; } = null!;

    public IEnumerable<Data<object>> Generate(IImmutableList<SessionData> sessionDataList,
        IImmutableList<DataSource> dataSourceList) => dataToGenerate;
}