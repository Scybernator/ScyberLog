using System;

namespace ScyberLog
{
    public class LogFormatterException : Exception
    {
        private readonly string _stackTrace;
        public override string StackTrace => this._stackTrace;
        public LogFormatterException(string message, string stackTrace, Exception innerException) : base(message, innerException)
        {
            _stackTrace = stackTrace;
        }
    }
}