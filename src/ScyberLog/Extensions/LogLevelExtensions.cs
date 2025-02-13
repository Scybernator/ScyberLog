using Microsoft.Extensions.Logging;

namespace ScyberLog
{
    internal static class LogLevelExtensions
    {
        public static string ToShortString(this LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRIT",
                LogLevel.None => "NONE ",
                _ => ""
            };
        }
    }
}