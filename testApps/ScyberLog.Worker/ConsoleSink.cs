using System;
using ScyberLog.Sinks;

namespace ScyberLog
{
    public class ExampleConsoleSink : ILogSink
    {
        public string Key => "example_console";

        public void Write<TState>(string message, LogContext<TState> state)
        {
            Console.WriteLine(message);
        }
    }
}