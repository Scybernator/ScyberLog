using System;

namespace ScyberLog
{
    public class LogSinkException : Exception
    {
        private readonly string _stackTrace;
        public override string StackTrace => this._stackTrace;
        public LogSinkException(string message, string stackTrace, Exception innerException) : base(message, innerException) 
        { 
            _stackTrace = stackTrace;
        }
    }
}