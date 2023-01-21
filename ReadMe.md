# ScyberLog
ScyberLog is a low-nonsense logging framework designed to meet a set of specific requirements:

- Simple configuration
- Rolling file logger that supports JSON formatted output
- Integration with .NET core logging extensions **without** excluding parameters which don't appear in the message template.
- Support for log scopes

This framework isn't intended to compete with the other big logging frameworks out there in terms of performance and configurability. Instead it aims to fill a gap in the available options by providing something simpler for small projects.  If you need something to drop in a pet project that doesn't require you to read a novel of documentation, download a bunch of 3rd party appenders and sinks (each with their own usage docs), and hand roll formatters just to get your logs into a json formatted file, then this the framework for you.  And since it only relies on the built in logging interfaces in .NET Core, you can always swap it out without changing your codebase once you outgrow it.

## Output
By default, ScyberLog writes log entries to both the console and a file. 
A log message like this:
```CSharp
_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
```
will produce a console line like this:
```
[19:17:36:7031] [INFO]  LoggerName - Worker running at: 01/26/2022 19:17:36 -06:00
```
File entries will be writen out as unformatted json by default, with one entry per line:
```Json
{"timeStamp":"2022-01-26T20:36:30.3726942-06:00","logger":"LoggerName","level":"DEBUG","message":"Worker running at: 01/26/2022 20:36:30 -06:00","state":{"data":{"time":"2022-01-26T20:36:30.3725537-06:00"}},"scopes":[{"scope":1},{"scope":2}]}
{"timeStamp":"2022-01-26T20:36:30.3733226-06:00","logger":"LoggerName","level":"INFO","message":"Worker running at: 01/26/2022 20:36:30 -06:00","state":{"data":{"time":"2022-01-26T20:36:30.3732356-06:00"},"values":[{"extraData":"HelloWorld"}]},"scopes":[{"scope":1},{"scope":2}]}
{"timeStamp":"2022-01-26T20:36:30.3737131-06:00","logger":"LoggerName","level":"WARN","message":"Worker running at: 01/26/2022 20:36:30 -06:00","state":{"data":{"time":"2022-01-26T20:36:30.3736427-06:00"}}}
{"timeStamp":"2022-01-26T20:36:30.3744115-06:00","logger":"LoggerName","level":"ERROR","message":"An error occurred during execution at 01/26/2022 20:36:30 -06:00","state":{"data":{"time":"2022-01-26T20:36:30.3743389-06:00"}},"exception":{"message":"Exceptional!","data":{},"hResult":-2146233088}}
{"timeStamp":"2022-01-26T20:36:30.3747746-06:00","logger":"LoggerName","level":"CRIT","message":"Worker running at: 01/26/2022 20:36:30 -06:00","state":{"data":{"time":"2022-01-26T20:36:30.3747104-06:00"}}}
```

You can configure the logger to format the json, if you prefer:
```Json
{
    "timeStamp": "2022-01-26T20:36:30.3733226-06:00",
    "logger": "LoggerName",
    "level": "INFO",
    "message": "Worker running at: 01/26/2022 20:36:30 -06:00",
    "state": {
        "data": {
            "time": "2022-01-26T20:36:30.3732356-06:00"
        },
        "values": [
            {
                "extraData": "HelloWorld"
            }
        ]
    },
    "scopes": [
        {
            "scope": 1
        },
        {
            "scope": 2
        }
    ]
}
```

## Usage
First, use the included extension to add the logger to the logging builder in the host builder; this will be very slightly different depending on your environment.

Worker Service:
```CSharp
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        //Clear out the console provider automatically added by the hosted service
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog();
    })
    .Build();
```

ASP.NET 6.0:
```CSharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog();
    });
```

Then inject the logger wherever you need it:
```CSharp
public WeatherForecastController(ILogger<WeatherForecastController> logger)
{
    _logger = logger;
    _logger.LogInformation("{Controller} initialized", nameof(WeatherForecastController));
}
```

There are some [example apps](https://github.com/Scybernator/ScyberLog/tree/master/testApps) in the solution.
## Unused Log Message Parameters
```CSharp
_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now, new { ExtraData = "HelloWorld"});
```
If you log a message like the above in another framework, you'll get a compiler warning about the number of parameters supplied to the logging extention methods.  This is because by default, .NET throws away parameters that aren't interpolated into the message when the message is written.  ScyberLog takes the stance that this information shouldn't be lost, and behaves differently.  If you intend to take advantage of this feature, know that if you later switch to another framework, this information will be lost. Avoid it if you want to preserve the ability to switch logging frameworks in the future without a change in behavior.

Including the following line in your project will remove the compiler warnings.
```CSharp
using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage("Usage", "CA2017:Number of parameters supplied in the logging message template do not match the number of named placeholders", Justification = "ScyberLog captures unused parameters")]
```
## Configuration
You can configure the logger by either using adding a `ScyberLog` node to your appsettings.json file and passing your `IConfiguration` instance to the `AddScyberLog` method, by using the optional configuration action parameter in the `AddScyberLog` extension, or by using the `.Configure<ScyberLogConfiguration>()` extension on your service collection.

Using `appsettings.json`
```Json 
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.Extensions.Hosting.Internal.Host" : "Information"
    }
  },
  "ScyberLog": {
    "EnableConsole": true,
    "EnableFile": true,
    "FileNameTemplate": "Log\\{0:yyyy-MM-dd}.log",
    "FileFormatter": "Json"
  }
}
```
```CSharp
builder.Host.ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog(hostingContext.Configuration);
    });
```

Using configuration action parameter:
```CSharp
builder.Host.ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog(config =>
        {
            config.FileFormatter = "console";
            config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    });
```

Using `services.Configure`:
```CSharp
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.Configure<ScyberLogConfiguration>(config =>
        {
            config.FileFormatter = "console";
            config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    })
    .ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog();
    })
    .Build();
```

**NOTE** Ironically, not all properties of the `JsonSerializerOptions` are serializable from JSON, so if you aren't happy with the defaults you'll need to configure them in code.  The default configuration is below; note that as of this writing the built in serializer fails to serialize `System.Exception` and hence the library has a custom converter.
```CSharp
public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
    {
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    }.With(x => x.Converters.Add(new JsonExceptionConverter<Exception>()));
```

For more information see [Options Pattern in .Net](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)

## Rolling Files
Scyberlog "supports" datetime based file rolling, mainly by interpolating the date into the filename when writing to the log.  The default file name is `"Log\\{0:yyyy-MM-dd}.log"`.  The Filename template is a standard format string, with the 0th parameter being the current datetime and the 1st parameter being the logger name.  Here is the implementation:
```CSharp
var path = string.Format(this.FileTemplate, DateTime.Now, state.Logger);
```

## Custom formatters and sinks
Loggers in ScyberLog are a composition of a message formatter and some number of mesage sinks.  If you need to modify the format of any log messages or write to another datasource you can implement a new [ILogFormatter](https://github.com/Scybernator/ScyberLog/blob/master/src/ScyberLog/Formatters/ILogFormatter.cs) or [ILogSink](https://github.com/Scybernator/ScyberLog/blob/master/src/ScyberLog/Sinks/ILogSink.cs) respectively and register them with the service collection.  Both of these interfaces implement the [IKeyedItem](https://github.com/Scybernator/ScyberLog/blob/246c6335e99b3d31ea7ff9a0e74d2219494c08d9/src/ScyberLog/Utils/KeyedItemCollection.cs) interface, which you can use to specify a string key with which you can reference your sinks/formatters in the configuration:

```CSharp
    services.AddTransient<ILogSink, MySink>();
    services.AddTransient<ILogFormatter, MyFormatter>();
    services.Configure<ScyberLogConfiguration>(config =>
    {
        config.EnableFile = false;
        config.EnableConsole = false;
        config.AdditionalLoggers.Add(
            new LoggerSetup()
            {
                Formatter = "my_formatter",
                Sinks = new { "my_sink", "file" }
            }
        );
    });
```

ScyberLog has two buit in formatters, `"text"` and `"json"` and three built-in sinks,`"console"`, `"colored_console"` and `"file"`.

## Performance
ScyberLog probably isn't fast. I haven't measured it. I can't recommend you use it for anything performance critical.