using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            //we permit one level of recursion to allow the framework to log formatting/sink errors,
            //while preventing loops due to recursive exceptions and other shenanigans
            this.LogInternal(logLevel, eventId, state, exception, formatter, terminal: false);
        }
        #endregion

        private void LogInternal<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter, bool terminal)
        {
            if (!IsEnabled(logLevel)) { return; }

            var (mappedState, mappedFormatter) = this.MapState(state, formatter);
            var context = GetLogContext(logLevel, eventId, exception, mappedState, mappedFormatter);

            var message = string.Empty;
            try
            {
                message = this.Formatter.Format(context);
            }
            // If we throw an exception while formatting, we attempt here to
            // a) convey as best we can the source of the problem; this means grabbing the stack trace so
            //   the user can track down the bad log call
            // b) report the error in the desired format to allow consumers of the log stream to parse
            //   these errors in the same way the parse normal logs (i.e. don't want to send a plaintext message
            //   to something expecting json)
            catch (FormatException ex) when (!terminal && state.IsFormattedLogValues())
            {
                // The below considerations were obsoleted by the below change to the LogValuesFormatter class
                // as of dotnet 8, which throws the parsing exception earlier in the call stack, before we get
                // get a chance to catch it.  Leaving it here in case they ever see the error of their ways.
                // https://github.com/dotnet/runtime/commit/8798c0459a36463bf3355f1059ad97fdd890c99e#diff-85963522e594a4a2ced0779745f1c5f219f0c017ed0743a7b98916dee71713f3R47

                // we enter this block when the user inputs an invalid format string,
                // typically due to mismatched braces (e.g "Hello {{world}!")
                var errorMessage = $"Error formatting log message. Format string: [{state.GetOriginalMessage()}]";
                var formattingException = new LogFormatterException(errorMessage, new StackTrace(fNeedFileInfo: true).ToString(), ex);
                // Here we override the formatter, since the old one was bad
                this.LogInternal<TState>(LogLevel.Warning, eventId, default, formattingException, (_, ex) => ex.Message, terminal: true);
                return;
            }
            catch (Exception ex) when (!terminal)
            {
                // typically we enter this block due to an object in the state not being serializable.
                var errorMessage = $"Error formatting log message.";
                errorMessage += " This is most often the result of a missing json converter. See Inner Exception for more details.";
                var formattingException = new LogFormatterException(errorMessage, new StackTrace(fNeedFileInfo: true).ToString(), ex);
                this.LogInternal(LogLevel.Warning, eventId, default, formattingException, mappedFormatter, terminal: true);
                return;
            }//if terminal = true, and an exception gets thrown by the formatter, it will bubble up if not squelched

            var sinkExceptions = new List<Exception>();
            foreach (var sink in this.Sinks)
            {
                try
                {
                    sink.Write(message, context);
                }
                catch (Exception ex) when (!terminal)
                {
                    var errorMessage = $"Error writing log message to sink. See Inner Exception for more details";
                    var sinkException = new LogSinkException(errorMessage, new StackTrace(fNeedFileInfo: true).ToString(), ex);
                    this.LogInternal(LogLevel.Warning, eventId, default, sinkException, mappedFormatter, terminal: true);
                    sinkExceptions.Add(ex);
                }
                catch (Exception ex)
                {
                    sinkExceptions.Add(ex);
                }
            }

            if (!terminal && sinkExceptions.Any())
            {
                throw new AggregateException("One or more errors occurred while writing to log sinks.", sinkExceptions);
            }
        }

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
