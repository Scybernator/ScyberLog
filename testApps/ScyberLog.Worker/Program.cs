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

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddTransient<ILogSink, ExampleConsoleSink>();
        services.AddTransient<ILogFormatter, SarcasticTextFormatter>();
        services.Configure<ScyberLogConfiguration>(config =>
        {
            config.EnableConsole = false;
            config.FileFormatter = "json";
            config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            config.AdditionalLoggers.Add(new LoggerSetup()
            {
                Formatter = "sarcastic",
                Sinks = new [] { "example_console" }
            });
        });
    })
    .ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog();
    })
    .Build();

await host.RunAsync();

