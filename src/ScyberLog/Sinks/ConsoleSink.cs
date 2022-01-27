using System;

namespace ScyberLog.Sinks
{
    public class ConsoleSink : ILogSink
    {
        public string Key => "console";
        private static object Lock { get; } = new Object();
        public void Write<TState>(string message, LogContext<TState> state)
        {
            lock(Lock)
            {
                Console.WriteLine(message);
            }
        }
    }
}