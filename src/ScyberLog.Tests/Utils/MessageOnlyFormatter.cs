using ScyberLog.Formatters;

namespace ScyberLog.Tests
{
    public class MessageOnlyFormatter : ILogFormatter
    {
        public string Key => string.Empty;

        public string Format<TState>(LogContext<TState> context)
        {
            return context.Formatter != null
                ? context.Formatter(context.State, context.Exception)
                : context.State?.ToString();
        }
    }
}