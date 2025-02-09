using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScyberLog.Formatters;
using static Microsoft.Extensions.Logging.LogLevel;

namespace ScyberLog.Tests
{
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

        //Test that we avoid a loop condition in formatter exception handling
        [TestMethod]
        public void FormatterExceptionDoesNotCauseInfiniteLoop()
        {
            var sink = new TestSink();
            var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

            var message = "{TimeStamp}} {Kilo}";//mismatched curly braces
            var param1 = DateTime.Now;
            var param2 = 1000;

            int timeout = 1000;
            var task = Task.Run(() => logger.Log(Information, default(EventId), default(Exception), message, param1, param2));;
            if(!task.Wait(timeout))
            {
                // Probabaly bad things happen if this test fails because the task could still be running
                // but at least we'll know about it. Check your process monitor for orphaned test hosts
                Assert.Fail("Timeout exceeded");
            }
        }

        [TestMethod]
        [Ignore("Obsoleted by change dotnet framework")]
        //https://github.com/dotnet/runtime/commit/8798c0459a36463bf3355f1059ad97fdd890c99e#diff-85963522e594a4a2ced0779745f1c5f219f0c017ed0743a7b98916dee71713f3R47
        public void FormatterExceptionsAreLogged()
        {
            var sink = new TestSink();
            var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

            //mismatched curly braces
            logger.Log(Information, default(EventId), default(Exception), "{TimeStamp {Kilo", DateTime.Now, 1000);
            Assert.IsTrue(sink.Contexts.Select(x => x.Exception).OfType<LogFormatterException>().Any(), "Log does not contain formatter exception");
        }
    }
}