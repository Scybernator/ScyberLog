using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScyberLog.Worker;
using ScyberLog;
using Microsoft.Extensions.Logging;
using System.Text.Json;

using System.Diagnostics.CodeAnalysis;
using ScyberLog.Sinks;
using ScyberLog.Formatters;

[assembly: SuppressMessage("Usage", "CA2017:Number of parameters supplied in the logging message template do not match the number of named placeholders", Justification = "ScyberLog captures unused parameters")]

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<ILogSink, ExampleConsoleSink>();
builder.Services.AddTransient<ILogFormatter, SarcasticTextFormatter>();
builder.Services.Configure<ScyberLogConfiguration>(config =>
{
    config.EnableConsole = false;
    config.FileFormatter = "json";
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.AdditionalLoggers.Add(new LoggerSetup()
    {
        Formatter = "sarcastic",
        Sinks = ["example_console"]
    });
});
builder.Logging.ClearProviders();
builder.Logging.AddScyberLog();

var host = builder.Build();

await host.RunAsync();

