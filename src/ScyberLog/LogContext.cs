using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ScyberLog
{
    public record LogContext
    {
        public DateTime TimeStamp { get; init; }
        public string Logger { get; init; } = string.Empty;
        public LogLevel LogLevel { get; init; } 
        public EventId? EventId { get; init; } 
        public Exception Exception { get; init; }  
        public IEnumerable<object> Scopes { get; init; } = Array.Empty<object>();
    }

    public record LogContext<TState> : LogContext
    {
        public TState State { get; init; } 
        [JsonIgnore]
        public Func<TState, Exception, string> Formatter { get; init; } 
    }
}