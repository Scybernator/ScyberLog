using System.Collections.Generic;

namespace ScyberLog
{
    public class LoggerSetup
    {
        public string Formatter { get; set; }
        public IEnumerable<string> Sinks { get; set; }
    }
}