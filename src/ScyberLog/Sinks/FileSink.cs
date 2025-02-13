using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Options;

namespace ScyberLog.Sinks
{
    public class FileSink : ILogSink
    {
        public string Key => "file";

        public string FileTemplate { get; }

        private static ConcurrentDictionary<string, object> Locks { get; } = new ConcurrentDictionary<string, object>();

        public FileSink(IOptions<ScyberLogConfiguration> config)
        {
            this.FileTemplate = config.Value.FileNameTemplate;
        }

        public void Write<TState>(string message, LogContext<TState> state)
        {
            //TODO ALY: this is kind of an unsatisfying way to handle this, but proper 
            //substitution would have to be hand rolled and you know how I hate that
            var path = string.Format(this.FileTemplate, DateTime.Now, state.Logger);
            path = Environment.ExpandEnvironmentVariables(path);//expand variables like %PROGRAMDATA%
            var fileInfo = new FileInfo(path);
            fileInfo.Directory.Create();
            lock (Locks.GetOrAdd(path, new object()))
            {
                File.AppendAllLines(path, [message]);
            }
        }
    }
}