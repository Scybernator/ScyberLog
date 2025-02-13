using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using ScyberLog.Formatters;
using ScyberLog.Sinks;

namespace ScyberLog
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddScyberLog(this ILoggingBuilder builder)
        {
            builder.Services.AddTransient<ILogSink, ConsoleSink>();
            builder.Services.AddTransient<ILogSink, ColoredConsoleSink>();
            builder.Services.AddTransient<ILogSink, FileSink>();
            builder.Services.AddTransient<ILogFormatter, TextFormatter>();
            builder.Services.AddTransient<ILogFormatter, JsonFormatter>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ScyberLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IStateMapper, FormattedLogValuesMapper>());
            LoggerProviderOptions.RegisterProviderOptions<ScyberLogConfiguration, ScyberLoggerProvider>(builder.Services);
            return builder;
        }

        public static ILoggingBuilder AddScyberLog(this ILoggingBuilder builder, Action<ScyberLogConfiguration> configure = null)
        {
            builder.AddScyberLog();
            if (configure != null)
            {
                builder.Services.PostConfigure(configure);
            }
            return builder;
        }

        public static ILoggingBuilder AddScyberLog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            builder.AddScyberLog();
            builder.Services.Configure<ScyberLogConfiguration>(configuration.GetRequiredSection("ScyberLog"));
            return builder;
        }

        public static ILoggingBuilder AddScyberLog(this ILoggingBuilder builder, IConfigurationSection namedConfigurationSection)
        {
            builder.AddScyberLog();
            builder.Services.Configure<ScyberLogConfiguration>(namedConfigurationSection);
            return builder;
        }

        public static ILoggingBuilder AddScyberLog(this ILoggingBuilder builder, IConfiguration namedConfigurationSection, Action<ScyberLogConfiguration> configure = null)
        {
            builder.AddScyberLog(configure);
            builder.Services.Configure<ScyberLogConfiguration>(namedConfigurationSection);
            return builder;
        }
    }
}