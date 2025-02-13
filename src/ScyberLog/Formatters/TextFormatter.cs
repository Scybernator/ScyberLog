namespace ScyberLog.Formatters
{
    public class TextFormatter : ILogFormatter
    {
        public string Key => "text";

        public string Format<TState>(LogContext<TState> context)
        {
            var message = context.Formatter != null ? context.Formatter(context.State, context.Exception) : context.State?.ToString();
            var level = $"[{context.LogLevel.ToShortString() + "]",-6}";
            return $"[{context.TimeStamp:HH:mm:ss:ffff}] {level} {context.Logger} - {message}";
        }
    }
}