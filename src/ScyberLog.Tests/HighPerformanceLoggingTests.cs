using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScyberLog.Formatters;
using static Microsoft.Extensions.Logging.LogLevel;

namespace ScyberLog.Tests
{
    //https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging
    [TestClass]
    public class HighPerformanceLoggingTests
    {
        private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
        private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));
        private ILogFormatter Formatter = new MessageOnlyFormatter();

        //check that LoggerMessage.Define doesn't do anything weird
        [TestMethod]
        public void LoggerMessageDefine()
        {
            var exception = new Exception("Exceptional!");
            var loggerName = "TestLogger";
            var sinks = new[]{ new TestSink((message, context) =>
                {
                    Assert.AreEqual(loggerName, context.Logger);
                    Assert.AreEqual("This is a test Foo", message);
                    Assert.AreEqual(42, context.EventId);
                    Assert.AreEqual(exception, context.Exception);
                })
            };
            var logger = new ScyberLogger(loggerName, Information, this.Formatter, sinks, this.StateMapper);

            var precompiledLogMessage = LoggerMessage.Define<string>(Trace, eventId: 42, "This is a test {Message}");
            precompiledLogMessage(logger, "Foo", exception);
        }

        //check that LoggerMessage.DefineScope doesn't do anything weird
        [TestMethod]
        public void LoggerMessageDefineScope()
        {
            var dt = DateTime.Now;
            var loggerName = "TestLogger";
            var sinks = new[]{ new TestSink((message, context) =>
                {
                    Assert.AreEqual(loggerName, context.Logger);
                    Assert.AreEqual("This is only a test", message);
                    Assert.AreEqual($"Scope started at: [{dt:s}]", context.Scopes.First().ToString());
                })
            };
            var logger = new ScyberLogger(loggerName, Information, Formatter, sinks, this.StateMapper);

            var timestampedScope = LoggerMessage.DefineScope<DateTime>("Scope started at: [{DateTime:s}]");
            using var scope = timestampedScope(logger, dt);

            logger.LogInformation("This is only a test");
        }
    }
}