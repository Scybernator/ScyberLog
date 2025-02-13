using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ScyberLog.Formatters
{
    public class JsonFormatter : ILogFormatter
    {
        public string Key => "json";

        public JsonSerializerOptions SerializationOptions { get; set; }

        public JsonFormatter(IOptions<ScyberLogConfiguration> config)
        {
            this.SerializationOptions = config.Value.JsonSerializerOptions;
        }

        private struct JsonLogContext
        {
            public DateTime TimeStamp { get; init; }
            public string Logger { get; init; }
            public string Level { get; init; }
            public string Message { get; init; }
            public EventId? EventId { get; init; }
            public object State { get; init; }
            public Exception Exception { get; init; }
            public IEnumerable<Object> Scopes { get; init; }
        }

        public string Format<TState>(LogContext<TState> context)
        {
            var logObject = new JsonLogContext
            {
                TimeStamp = context.TimeStamp,
                Logger = context.Logger,
                Level = context.LogLevel.ToShortString(),
                Message = context.Formatter != null ? context.Formatter(context.State, context.Exception) : null,
                EventId = context.EventId,
                State = context.State,
                Exception = context.Exception,
                Scopes = context.Scopes.Any() ? context.Scopes : null,
            };

            return JsonSerializer.Serialize(logObject, this.SerializationOptions);
        }
    }
}