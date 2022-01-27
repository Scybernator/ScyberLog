using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Scyberlog.Utils;

namespace ScyberLog
{
    public class ScyberLogConfiguration
    {
        public bool EnableConsole { get; set; } = true;
        public bool EnableFile { get; set; } = true;
        public string ConsoleFormatter { get; set; } = "text";
        public string FileFormatter { get; set; } = "json";
        public string FileNameTemplate { get; set; } = "Log\\{0:yyyy-MM-dd}.log";
        public LogLevel MinLevel { get; set; } = LogLevel.Debug;
        public bool IncludeOriginalFormat { get; set; } = false;
        public bool ThrowScyberLogExceptions { get; set; } = false;
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        }.With(x => x.Converters.Add(new JsonExceptionConverter<Exception>()));
        public ICollection<LoggerSetup> AdditionalLoggers { get; set; } = new List<LoggerSetup>();
    }
}