using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ScyberLog.Sinks
{
    public class ColoredConsoleSink : ILogSink
    {
        public string Key => "colored_console";

        public void Write<TState>(string message, LogContext<TState> state)
        {
            using(new ColoredConsole()) {
                if(ConsoleColors.ContainsKey(state.LogLevel))
                {
                    var colors = ConsoleColors[state.LogLevel];
                    Console.ForegroundColor = colors.Foreground;
                    Console.BackgroundColor = colors.Background;
                }
                Console.WriteLine(message);
            }
        }

        private readonly Dictionary<LogLevel, (ConsoleColor Foreground, ConsoleColor Background)> ConsoleColors = new ()
        {
            {LogLevel.Critical, (ConsoleColor.White, ConsoleColor.Red)},
            {LogLevel.Error, (ConsoleColor.DarkRed, ConsoleColor.Black)},
            {LogLevel.Warning, (ConsoleColor.DarkYellow, ConsoleColor.Black)},
            {LogLevel.Information, (ConsoleColor.White, ConsoleColor.Black)},
            {LogLevel.Debug, (ConsoleColor.DarkGreen, ConsoleColor.Black)}
        };
    }
}