using System;
using System.Linq;
using ScyberLog.Formatters;

namespace ScyberLog
{
    public class SarcasticTextFormatter : ILogFormatter
    {
        public string Key => "sarcastic";
        public Random Random = new();
        public string Format<TState>(LogContext<TState> context)
        {
            var formattedMessage = context.Formatter != null ? context.Formatter(context.State, context.Exception) : context.State?.ToString();
            return formattedMessage.Aggregate("", (string a, char x) => a + (Random.Next(2) != 0 ? x.ToString().ToUpper() : x.ToString().ToLower()));
        }
    }
}