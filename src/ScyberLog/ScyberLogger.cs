using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ScyberLog.Formatters;
using ScyberLog.Sinks;

[assembly: InternalsVisibleTo("ScyberLog.Tests")]

namespace ScyberLog 
{
    internal class ScyberLogger : ILogger
    {
        private string Name { get; }
        private ILogFormatter Formatter { get; }
        private IEnumerable<ILogSink> Sinks { get; }
        private LogLevel MinLevel { get; }
        private IStateMapper StateMapper { get; }
        private ConcurrentDictionary<Guid, object> Scopes { get; } = new ConcurrentDictionary<Guid, object>();

        public ScyberLogger(string name, LogLevel minLevel, ILogFormatter formatter, IEnumerable<ILogSink> sinks, IStateMapper stateMapper)
        {
            this.Name = name;
            this.Formatter = formatter;
            this.Sinks = sinks;
            this.MinLevel = minLevel;
            this.StateMapper = stateMapper;
        }

        #region ILogger
        public IDisposable BeginScope<TState>(TState state)
        {
            var guid = Guid.NewGuid();
            var (scope, _) = MapState(state, (x, ex) => state?.ToString());
            this.Scopes[guid] = scope;
            return new DisposeAction(() => this.Scopes.Remove(guid, out _));
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= this.MinLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) { return; }

            var (mappedState, mappedFormatter) = this.MapState(state, formatter);
            var context = GetLogContext(logLevel, eventId, exception, mappedState, mappedFormatter);

            var message = string.Empty;
            try
            {
                message = this.Formatter.Format(context);
            }catch(Exception ex)
            {
                message = "Error formatting log message; " + ex.Message;
                if(ex is FormatException && state.IsFormattedLogValues())
                {
                    message += " Format string: [" + state.GetOriginalMessage() + "]";
                }
            }

            this.Sinks.ForEach(x => x.Write(message, context));
        }
        #endregion

        private (object mappedState, Func<object, Exception, string> MappedFormatter) MapState<TState>(TState state, Func<TState, Exception, string> formatter)
        {
            object mappedState = state;
            Func<object, Exception, string> mappedFormatter = formatter != null ? (_, ex) => formatter(state, ex) : null;
            if (this.StateMapper.Map(state, formatter, out object outObject, out Func<object, Exception, string> outFormatter))
            {
                mappedState = outObject;
                mappedFormatter = outFormatter;
            }
            return (mappedState, mappedFormatter);
        }

        private LogContext<TState> GetLogContext<TState>(LogLevel logLevel, EventId eventId, Exception exception, TState state, Func<TState, Exception, string> formatter)
        {
            return new LogContext<TState>()
            {
                TimeStamp = DateTime.Now,
                Logger = this.Name,
                LogLevel = logLevel,
                EventId = (eventId.Id != default(EventId).Id || eventId.Name != default(EventId).Name) ? eventId : null,
                State = state,
                Exception = exception,
                Formatter = formatter,
                Scopes = this.Scopes.Values
            };
        }
    }
}
