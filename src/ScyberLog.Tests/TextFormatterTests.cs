using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScyberLog.Formatters;
using static Microsoft.Extensions.Logging.LogLevel;

namespace ScyberLog.Tests;

[TestClass]
public class TextFormatterTests
{
    private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
    private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));
    private ILogFormatter Formatter = new TextFormatter();

    [TestMethod]
    public void NullChecks()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Trace, Formatter, new []{ sink }, this.StateMapper);
        var logMessage = new{
            TimeStamp = DateTime.Now,
            Message = "Hello World",
            Log = "TestLogger",
            Source = "Application"
        };
        logger.Log(logLevel: Information, eventId: default, state: logMessage, exception: null, formatter: null);
    }
}