using System;
using ScyberLog.Formatters;

namespace ScyberLog.Tests
{
    internal class TestFormatter : ILogFormatter
    {
        public string Key => string.Empty;
        public TestFormatter(Func<LogContext, string> formatter = null)
        {
            this.Formatter = formatter ?? ((context) => context.ToString());
        }

        private Func<LogContext, string> Formatter { get; }

        public string Format<TState>(LogContext<TState> context) => this.Formatter(context);
    }
}