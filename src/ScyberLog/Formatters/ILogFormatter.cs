namespace ScyberLog.Formatters
{
    public interface ILogFormatter : IKeyedItem
    {
        string Format<TState>(LogContext<TState> context);
    }
}