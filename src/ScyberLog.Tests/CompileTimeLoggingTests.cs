using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScyberLog.Formatters;
using static Microsoft.Extensions.Logging.LogLevel;

namespace ScyberLog.Tests
{
    //https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator
    public static partial class Log
    {
        [LoggerMessage(EventId = 8, Level = Critical, Message = "{Thing1}{Thing2}{Thing3}")]
        public static partial void LogMessage(
            this ILogger logger,
            string thing1,
            string thing2,
            string thing3,
            Exception ex);
    }

    //Verify that compile time logging generators don't do anything weird.
    [TestClass]
    public class CompileTimeLoggingTests
    {
        private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
        private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));
        private ILogFormatter Formatter = new MessageOnlyFormatter();

        [TestMethod]
        public void CompileTimeLogging()
        {
            var exception = new Exception("Exceptional!");
            var loggerName = "TestLogger";

            var sinks = new[]{ new TestSink((message, context) =>
                {
                    Assert.AreEqual(loggerName, context.Logger);
                    Assert.AreEqual("🙈🙉🙊", message);
                    Assert.AreEqual(8, context.EventId);
                    Assert.AreEqual(exception, context.Exception);
                })
            };
            var logger = new ScyberLogger(loggerName, Information, this.Formatter, sinks, this.StateMapper);
            logger.LogMessage("🙈", "🙉", "🙊", exception);
        }
    }
}