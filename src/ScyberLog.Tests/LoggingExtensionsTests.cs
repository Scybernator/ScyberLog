using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Extensions.Logging.LogLevel;

[assembly: SuppressMessage("Usage", "CA2017:Number of parameters supplied in the logging message template do not match the number of named placeholders", Justification = "Exactly the problem remedied by this library")]

namespace ScyberLog.Tests
{
    [TestClass]
    public class LoggingExtensionsTests
    {
        private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
        private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));

        //Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        [TestMethod]
        public void UsedParametersAppearInState()
        {
            var formatter = new TestFormatter();
            var sink = new TestSink();
            var logger = new ScyberLogger(string.Empty, Information, formatter, new []{ sink }, this.StateMapper);

            var message = "{TimeStamp} {Kilo}";
            var param1 = DateTime.Now;
            var param2 = 1000;
            var param3 = TimeSpan.FromMilliseconds(10000);
            logger.Log(Information, default(EventId), default(Exception), message, param1, param2, param3);
            var state = (sink.States.First() as FormattedLogValuesWrapper)?.Data.Select(x => x.Value).ToList();
            CollectionAssert.Contains(state, param1, $"[{nameof(param1)}] missing from state");
            CollectionAssert.Contains(state, param2, $"[{nameof(param2)}] missing from state");
        }

        [TestMethod]
        public void UnusedParametersAppearInData()
        {
            var formatter = new TestFormatter();
            var sink = new TestSink();
            var logger = new ScyberLogger(string.Empty, Information, formatter, new []{ sink }, this.StateMapper);

            var message = "{TimeStamp}";
            var param1 = DateTime.Now;
            var param2 = 1000;
            var param3 = TimeSpan.FromMilliseconds(10000);
            logger.Log(Information, default(EventId), default(Exception), message, param1, param2, param3);
            var values = (sink.States.First() as FormattedLogValuesWrapper)?.Values.ToList();
            CollectionAssert.Contains(values, param2, $"[{nameof(param2)}] missing from data");
            CollectionAssert.Contains(values, param3, $"[{nameof(param3)}] missing from data");
        }

        //BeginScope(this ILogger logger, string messageFormat, params object[] args) //Converts to FormattedLogValues
        [TestMethod]
        public void UsedParametersAppearInScope()
        {
            var formatter = new TestFormatter();
            var sink = new TestSink();
            var param1 = DateTime.Now;
            var param2 = "Hello World!";
            var param3 = new { ExtraData = 1000 };
            var logger = new ScyberLogger(string.Empty, Information, formatter, new []{ sink }, this.StateMapper);
            using(var scope1 = logger.BeginScope("{TimeStamp} {Message}", param1, param2, param3))
            {
                logger.Log(Information, default(EventId), default(Exception), string.Empty);
                var scope = sink.Scopes.First() as FormattedLogValuesWrapper;
                var scopeData = scope?.Data.Values.ToList();
                CollectionAssert.Contains(scopeData, param1, $"[{nameof(param1)}] missing from scope");
                CollectionAssert.Contains(scopeData, param2, $"[{nameof(param2)}] missing from scope");
            }
        }

        [TestMethod]
        public void UnusedParametersAppearInScope()
        {
            var formatter = new TestFormatter();
            var sink = new TestSink();
            var param1 = DateTime.Now;
            var param2 = "Hello World!";
            var param3 = new { ExtraData = 1000 };
            var logger = new ScyberLogger(string.Empty, Information, formatter, new []{ sink }, this.StateMapper);
            using(var scope1 = logger.BeginScope("{TimeStamp}", param1, param2, param3))
            {
                logger.Log(Information, default(EventId), default(Exception), string.Empty);
                var scope = sink.Scopes.First() as FormattedLogValuesWrapper;
                var scopeValues = scope?.Values.ToList();
                CollectionAssert.Contains(scopeValues, param2, $"[{nameof(param2)}] missing from scope data");
                CollectionAssert.Contains(scopeValues, param3, $"[{nameof(param3)}] missing from scope data");
            }
        }
    }
}