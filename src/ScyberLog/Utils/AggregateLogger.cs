using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ScyberLog 
{
    internal class AggregateLogger : ILogger
    {
        private IEnumerable<ILogger> Loggers { get; }

        public AggregateLogger(IEnumerable<ILogger> loggers)
        {
            this.Loggers = loggers;
        }

        public IDisposable BeginScope<TState>(TState state) => new AggregateDisposable(this.Loggers.Select(x => x.BeginScope(state)).ToList());

        public bool IsEnabled(LogLevel logLevel) => this.Loggers.Any(x => x.IsEnabled(logLevel));

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.Loggers.ForEach(x => x.Log(logLevel, eventId, state, exception, formatter));
        }
    }
}