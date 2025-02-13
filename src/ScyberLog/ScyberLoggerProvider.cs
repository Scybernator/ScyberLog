using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScyberLog.Formatters;
using ScyberLog.Sinks;

namespace ScyberLog
{
    public class ScyberLoggerProvider : ILoggerProvider
    {
        private ScyberLogConfiguration Config { get; }
        private IStateMapper StateMapper { get; }
        private KeyedCollection<string, ILogSink> Sinks { get; }
        private KeyedCollection<string, ILogFormatter> Formatters { get; }

        public ScyberLoggerProvider(IOptions<ScyberLogConfiguration> config, IStateMapper stateMapper, IEnumerable<ILogSink> sinks, IEnumerable<ILogFormatter> formatters)
        {
            this.Config = config.Value;
            this.StateMapper = stateMapper;
            this.Sinks = new KeyedItemCollection<ILogSink>(sinks);
            this.Formatters = new KeyedItemCollection<ILogFormatter>(formatters);
        }

        public ILogger CreateLogger(string categoryName)
        {
            var loggers = new List<ILogger>();
            if (this.Config.EnableConsole)
            {
                var consoleLogger = this.BuildLogger(categoryName,
                    new LoggerSetup()
                    {
                        Formatter = this.Config.ConsoleFormatter ?? "text",
                        Sinks = ["colored_console"]
                    });
                loggers.Add(consoleLogger);
            }

            if (this.Config.EnableFile)
            {
                var consoleLogger = this.BuildLogger(categoryName,
                new LoggerSetup()
                {
                    Formatter = this.Config.FileFormatter ?? "json",
                    Sinks = ["file"]
                });
                loggers.Add(consoleLogger);
            }

            if (this.Config.AdditionalLoggers != null)
            {
                foreach (var setup in this.Config.AdditionalLoggers)
                {
                    loggers.Add(this.BuildLogger(categoryName, setup));
                }
            }
            var squelchedLoggers = loggers.Select(x => new SquelchedLogger(x, this.Config.ThrowScyberLogExceptions)).ToList();
            return new AggregateLogger(squelchedLoggers);
        }

        private ILogger BuildLogger(string categoryName, LoggerSetup setup)
        {
            if (!this.Formatters.TryGetValue(setup.Formatter, out ILogFormatter formatter))
            {
                throw new KeyNotFoundException($"Formatter [{setup.Formatter}] not registered.");
            }

            var sinks = new List<ILogSink>();
            foreach (var sinkKey in setup.Sinks)
            {
                if (!this.Sinks.TryGetValue(sinkKey, out ILogSink sink))
                {
                    throw new KeyNotFoundException($"Sink [{sink}] not registered.");
                }
                sinks.Add(sink);
            }

            return new ScyberLogger(
                    categoryName,
                    this.Config.MinLevel,
                    formatter,
                    sinks,
                    this.StateMapper);
        }

        public void Dispose() { }
    }
}