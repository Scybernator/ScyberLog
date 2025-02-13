using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScyberLog;
using ScyberLog.Formatters;
using ScyberLog.Sinks;
using ScyberLog.Worker;

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

