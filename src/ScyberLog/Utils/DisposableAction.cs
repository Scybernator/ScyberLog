using System;

namespace ScyberLog
{
    internal class DisposeAction : IDisposable
    {
        private Action Action { get; }
        private bool Disposed  { get; set; }
        private object Lock { get; } = new ();

        public DisposeAction(Action onDispose)
        {
            this.Action = onDispose;
        }

        public void Dispose() 
        {
            lock(this.Lock)
            {
                if(!this.Disposed)
                {
                    try
                    {
                        this.Action();
                    } finally
                    {
                        this.Disposed = true;
                    }
                }
            }
        }
    }
}