using System;
using System.Collections.Generic;
using ScyberLog.Sinks;

namespace ScyberLog.Tests;

internal class TestSink : ILogSink
{
    public string Key => string.Empty;

    private Action<string, LogContext> SinkAction { get; }

    public string LastMessage { get; private set; }
    public List<string> Messages { get; } = new List<string>();
    public List<LogContext> Contexts { get; } = new List<LogContext>();
    public List<object> States { get; } = new List<object>();
    public List<object> Scopes { get; } = new List<object>();

    public TestSink(Action<string, LogContext> sinkAction = null)
    {
        this.SinkAction = sinkAction ?? ((_, _) => {});
    }

    public void Write<TState>(string message, LogContext<TState> context)
    {
        this.LastMessage = message;
        this.Messages.Add(message);
        this.Contexts.Add(context);
        this.States.Add(context.State);
        this.Scopes.AddRange(context.Scopes);
        this.SinkAction(message, context);
    }
}