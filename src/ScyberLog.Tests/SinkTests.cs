using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Extensions.Logging.LogLevel;

namespace ScyberLog.Tests
{
    [TestClass]
    public class SinkTests
    {
        private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
        private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));

        [TestMethod]
        public void SinkFailureLoggedInSink()
        {
            var formatter = new TestFormatter();
            var sinkException = new Exception("Sink failed!");
            var testSink = new TestSink((_, context) => throw sinkException);
            var logger = new SquelchedLogger(new ScyberLogger(string.Empty, Information, formatter, [testSink], this.StateMapper), throwExceptions: false);
            logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
            Assert.IsTrue(testSink.Contexts.Any(x => x.Exception?.InnerException == sinkException), "Failed to handle exception in test sink.");
        }

        [TestMethod]
        public void SinkFailureDoesntEffectOtherSinks()
        {
            var formatter = new TestFormatter();
            var sinkException = new Exception("Sink failed!");
            var goofusSink = new TestSink((_, context) => throw sinkException);
            var gallantSink = new TestSink((_, context) => { });
            var sinks = new[] { goofusSink, gallantSink };
            var logger = new SquelchedLogger(new ScyberLogger(string.Empty, Information, formatter, sinks, this.StateMapper), throwExceptions: false);
            logger.Log(Information, default(EventId), "Hello World!", null, (_, _) => string.Empty);
            //Exception gets logged in good sink
            Assert.IsTrue(gallantSink.Contexts.Any(x => x.Exception?.InnerException == sinkException), "Failed to write exception in good sink.");
            //Message gets logged in gallant sink
            Assert.IsTrue(gallantSink.States.Count(x => x?.ToString() == "Hello World!") == 1, "Failed to write message in good sink.");
        }

        [TestMethod]
        public void SinkExceptionsAreThrownIfNotSquelched()
        {
            var formatter = new TestFormatter();
            var sinkException = new Exception("Sink failed!");
            var testSink = new TestSink((_, context) => throw sinkException);
            var logger = new ScyberLogger(string.Empty, Information, formatter, [testSink], this.StateMapper);
            Assert.ThrowsException<AggregateException>(() => logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty));
        }
    }
}