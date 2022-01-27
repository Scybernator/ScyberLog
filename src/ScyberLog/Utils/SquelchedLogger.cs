using System;
using Microsoft.Extensions.Logging;

namespace ScyberLog
{
    internal class SquelchedLogger : ILogger
    {
        private ILogger InternalLogger { get; }
        private bool ThrowExceptions { get; }

        public SquelchedLogger(ILogger logger, bool throwExceptions)
        {
            this.InternalLogger = logger;
            this.ThrowExceptions = throwExceptions;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                InternalLogger.Log(logLevel, eventId, state, exception, formatter);
            }catch(Exception)
            {
                if(ThrowExceptions)
                {
                    throw;
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            try 
            {
                return InternalLogger.IsEnabled(logLevel);
            }
            catch(Exception)
            {
                if(this.ThrowExceptions)
                {
                    throw;
                }
                return false;
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            try
            {
                return InternalLogger.BeginScope(state);
            }
            catch(Exception)
            {
                if(ThrowExceptions)
                {
                    throw;
                }
                return new DisposeAction(() => {});
            }
        }
    }
}