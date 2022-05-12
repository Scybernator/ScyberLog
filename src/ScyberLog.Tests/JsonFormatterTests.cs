using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScyberLog.Formatters;
using static Microsoft.Extensions.Logging.LogLevel;

namespace ScyberLog.Tests;

[TestClass]
public class JsonFormatterTests
{
    private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
    private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));
    private ILogFormatter Formatter = new JsonFormatter(Options.Create(Config));

    [TestMethod]
    public void ExceptionSerializesWithoutError()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

        try
        {
            throw new Exception("Exceptional!");
        }catch(Exception ex)
        {
            logger.LogError(ex, "An exception has occurred");
        }
    }

    [TestMethod]
    public void InnerExceptionsOnlyAppearOnceInAggregateExceptions()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);
        var exception1 = new Exception("Exception 1"); 
        var exception2 = new Exception("Exception 2"); 
        try
        {
            throw new AggregateException("Exceptional!", new [] { exception1, exception2});
        }catch(Exception ex)
        {
            logger.LogError(ex, "An exception has occurred");
        }
        var logMessage = sink.LastMessage;
        //messages appears twice, once in the aggregate message and once in inner exceptions
        Assert.AreEqual(2, Regex.Matches(logMessage, exception1.Message).Count, $"Inner exception appeared incorrect number of times: {logMessage}");
        Assert.AreEqual(2, Regex.Matches(logMessage, exception2.Message).Count, $"Inner exception appeared incorrect number of times: {logMessage}");
    }

    [TestMethod]
    public void UsedParametersAppearInState()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

        var message = "{Greeting} {Kilo}";
        var param1 = "HELLO";
        var param2 = 1000;
        var param3 = TimeSpan.FromMilliseconds(10000);
        logger.Log(Information, default(EventId), default(Exception), message, param1, param2, param3);
        var logMessage = sink.LastMessage;
        StringAssert.Contains(logMessage, param1.ToString(), $"[{nameof(param1)}] missing from log message");
        StringAssert.Contains(logMessage, param2.ToString(), $"[{nameof(param2)}] missing from log message");
    }

    [TestMethod]
    public void UnusedParametersAppearInData()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

        var message = "{TimeStamp}";
        var param1 = DateTime.Now;
        var param2 = 1000;
        var param3 = TimeSpan.FromMilliseconds(10000);
        logger.Log(Information, default(EventId), default(Exception), message, param1, param2, param3);
        var logMessage = sink.LastMessage;
        StringAssert.Contains(logMessage, param2.ToString(), $"[{nameof(param2)}] missing from log message");
        StringAssert.Contains(logMessage, param3.ToString(), $"[{nameof(param3)}] missing from log message");
    }

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

    [TestMethod]
    public void TextLogContainsNoState()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

        logger.LogInformation("There shouldn't be any state in the json");
        StringAssert.DoesNotMatch(sink.LastMessage, new Regex("\"state\":"), "State was present in text only message");
    }

    private class TokenException : Exception
    {
        public CancellationToken Token { get; set; }
        public TokenException(string message) : base(message) {}
    }

    [TestMethod]
    public void ExceptionsDoNotSerializeCancellationTokens()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);
        var exception = new TokenException("Exceptional!");
        logger.LogError(exception, "There shouldn't be any Cancellation Token here");
        StringAssert.DoesNotMatch(sink.LastMessage, new Regex("\"CancellationToken\":"), "CancellationToken rendered in json");
    }

    private class PropertyAccessException : Exception
    {
        public string BadProperty { get => throw new Exception("Exception accessing property"); }
        public PropertyAccessException(string message) : base(message) {}
    }

    [TestMethod]
    public void ExceptionsReportSerializationError()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);
        var exception = new PropertyAccessException("Exceptional!");
        logger.LogError(exception, string.Empty);
        StringAssert.DoesNotMatch(sink.LastMessage, new Regex("\"Exception accessing property\":"), "Exception not rendered in json");
    }

    [TestMethod]
    public void InvalidFormatStringHandling()
    {
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, Formatter, new []{ sink }, this.StateMapper);

        var message = "{TimeStamp} {{Kilo}";
        var param1 = DateTime.Now;
        var param2 = 1000;
        var param3 = TimeSpan.FromMilliseconds(10000);
        logger.Log(Information, default(EventId), default(Exception), message, param1, param2, param3);
        logger.LogInformation(message, param1, param2, param3);
        StringAssert.Contains(sink.LastMessage, "Error formatting log message");
    }
}