using System;
using System.Collections.Generic;
using System.Linq;

namespace ScyberLog
{
    internal class AggregateDisposable : IDisposable
    {
        private IEnumerable<IDisposable> Disposables { get; }

        public AggregateDisposable(IEnumerable<IDisposable> disposables)
        {
            this.Disposables = disposables ?? Array.Empty<IDisposable>();
        }

        public void Dispose()
        {
            var exceptions = new List<Exception>();
            foreach(var disposable in this.Disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch(Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            
            if(exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}