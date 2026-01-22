using QaaS.Framework.SDK;
using QaaS.Framework.SDK.ContextObjects;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace QaaS.Common.Assertions.Tests;

public static class Globals
{
    public static readonly ILogger Logger = new SerilogLoggerFactory(
        new LoggerConfiguration().MinimumLevel.Debug()
            .WriteTo.NUnitOutput()
            .CreateLogger()).CreateLogger("TestsLogger");

    public static readonly Context Context = new Context
    {
        Logger = Logger
    };
}