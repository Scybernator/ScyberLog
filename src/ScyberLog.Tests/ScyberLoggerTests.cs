using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Extensions.Logging.LogLevel;
namespace ScyberLog.Tests;

[TestClass]
public class ScyberLoggerTests
{
    private static ScyberLogConfiguration Config = new ScyberLogConfiguration();
    private IStateMapper StateMapper = new FormattedLogValuesMapper(Options.Create(Config));

    [TestMethod]
    public void LoggerNameIsPresent()
    {
        var loggerName = "TestLogger";
        var formatter = new TestFormatter();
        var sinks = new []{ new TestSink( (_, context) => Assert.AreEqual(loggerName, context.Logger)) };
        var logger = new ScyberLogger(loggerName, Information, formatter, sinks, this.StateMapper);
        logger.Log(Information, default(EventId), string.Empty, null, (_,_) => string.Empty);
    }

    [TestMethod]
    public void LoggerIsEnabledOnlyAboveMinLevel()
    {       
        var formatter = new TestFormatter();
        var sink = new TestSink();
        foreach(LogLevel minLevel in Enum.GetValues(typeof(LogLevel)))
        {
            var logger = new ScyberLogger(string.Empty, minLevel, formatter, new []{ sink }, this.StateMapper);
            foreach(LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {       
                Assert.IsTrue(logger.IsEnabled(level) == (level >= minLevel), $"Incorrect enablement: [{minLevel}/{level}]");
            }
        }
    }

    [TestMethod]
    public void LoggerLogsOnlyAboveMinLevel()
    {
        var formatter = new TestFormatter();
              
        foreach(LogLevel minLevel in Enum.GetValues(typeof(LogLevel)))
        {
            var expectedCount = 0;
            var sink = new TestSink((message, state) => Assert.IsTrue((state.LogLevel >= minLevel), $"Incorrect enablement: [{minLevel}/{state.LogLevel}]"));
            var logger = new ScyberLogger(string.Empty, minLevel, formatter, new []{ sink }, this.StateMapper);
            foreach(LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {       
                logger.Log(level, default(EventId), String.Empty, null, (_, _) => string.Empty);   
                if(level >= minLevel) { expectedCount++; };
                Assert.AreEqual(expectedCount, sink.Messages.Count(), $"Failed to write to sink: [{minLevel}/{level}]");           
            }
        }
    }

    [TestMethod]
    public void LogScopesDisposedCorrectly()
    {
        var formatter = new TestFormatter();
        var scopes = new List<object>();
        var sink = new TestSink((message, state) => scopes.AddRange(state.Scopes));
        var logger = new ScyberLogger(string.Empty, Trace, formatter, new []{ sink }, this.StateMapper);
        var scope1 = new { data = "Scope 1"};
        var scope2 = new { data = "Scope 2"};
        using (logger.BeginScope(scope1))
        {
            using (logger.BeginScope(scope2))
            {
                logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
                CollectionAssert.AreEquivalent(new []{ scope1, scope2 }, scopes, "One or more scopes were not present.");
                scopes.Clear();
            }
            logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
            CollectionAssert.AreEquivalent(new []{ scope1 }, scopes, "Scope was not present.");
            scopes.Clear(); 
        }
        logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
        Assert.IsTrue(!scopes.Any(), "Scopes were not disposed.");
    }

    [TestMethod]
    public void LogScopesDisposedIrresponsibly()
    {
        var formatter = new TestFormatter();
        var scopes = new List<object>();
        var sink = new TestSink((message, state) => scopes.AddRange(state.Scopes));
        var logger = new ScyberLogger(string.Empty, Trace, formatter, new []{ sink }, this.StateMapper);
        var scope1 = new { data = "Scope 1"};
        var disposable1 = logger.BeginScope(scope1);
            
        var count = 100;
        var tasks = Enumerable.Range(0, count).Select(x => 
        { 
            var task = new Task(() => disposable1.Dispose()); 
            Task.Delay((100 - x)*3);
            task.Start(); 
            return task; 
        }).ToArray();
        Task.WaitAll(tasks);
        
        logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
        Assert.IsTrue(!scopes.Any(), "Scopes were not disposed.");
    }

    [TestMethod]
    public void NullScopes()
    {
        var formatter = new TestFormatter();
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Trace, formatter, new []{ sink }, this.StateMapper);
        object scope1 = null;
        using (logger.BeginScope(scope1))
        {
            logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
            CollectionAssert.AreEquivalent(new []{ scope1 }, sink.Scopes, "Scope was not present.");
            sink.Scopes.Clear(); 
        }
        logger.Log(Information, default(EventId), string.Empty, null, (_, _) => string.Empty);
        Assert.IsTrue(!sink.Scopes.Any(), "Scopes were not disposed.");
    }

    [TestMethod]
    public void EventIdLogged()
    {
        var eventId = new EventId(0, "event");
        var formatter = new TestFormatter();
        var sinks = new []{ new TestSink( (_, context) => Assert.AreEqual(eventId, context.EventId)) };
        var logger = new ScyberLogger(string.Empty, Information, formatter, sinks, this.StateMapper);
        logger.Log(Information, eventId, string.Empty, null, (_,_) => string.Empty);
    }

    [TestMethod]
    public void DefaultEventIdIsNulled()
    {
        var eventId = default(EventId);
        var formatter = new TestFormatter();
        var sinks = new []{ new TestSink( (_, context) => Assert.AreEqual(null, context.EventId)) };
        var logger = new ScyberLogger(string.Empty, Information, formatter, sinks, this.StateMapper);
        logger.Log(Information, eventId, string.Empty, null, (_,_) => string.Empty);
    }

    [TestMethod]
    public void TestStateLogged()
    {
        var state = new { message = "Hello World!" };
        var formatter = new TestFormatter();
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, formatter, new []{ sink }, this.StateMapper);
        logger.Log(Information, default(EventId), state, null, (_,_) => string.Empty); 
        CollectionAssert.Contains(sink.States, state);       
    }

    [TestMethod]
    public void NullTestState()
    {
        object state = null;
        var formatter = new TestFormatter();
        var sink = new TestSink();
        var logger = new ScyberLogger(string.Empty, Information, formatter, new []{ sink }, this.StateMapper);
        logger.Log(Information, default(EventId), state, null, (_,_) => string.Empty); 
        CollectionAssert.Contains(sink.States, state);       
    }

    [TestMethod]
    public void ExceptionIsLogged()
    {
        var exception = new Exception("Exceptional!");
        var formatter = new TestFormatter();
        var sinks = new []{ new TestSink( (_, context) => Assert.AreSame(exception, context.Exception)) };
        var logger = new ScyberLogger(string.Empty, Information, formatter, sinks, this.StateMapper);
        logger.Log(Information, default(EventId), string.Empty, exception, (_,_) => string.Empty);
    }    

    [TestMethod]
    public void DefaultExceptionIsNulled()
    {
        var exception = default(Exception);
        var formatter = new TestFormatter();
        var sinks = new []{ new TestSink( (_, context) => Assert.AreSame(exception, context.Exception)) };
        var logger = new ScyberLogger(string.Empty, Information, formatter, sinks, this.StateMapper);
        logger.Log(Information, default(EventId), string.Empty, exception, (_,_) => string.Empty);
    }   
}