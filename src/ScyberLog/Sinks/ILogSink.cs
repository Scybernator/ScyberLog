namespace ScyberLog.Sinks
{
    public interface ILogSink : IKeyedItem
    {
        void Write<TState>(string message, LogContext<TState> state);
    }
}