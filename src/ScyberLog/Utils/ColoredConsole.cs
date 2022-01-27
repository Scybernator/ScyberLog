using System;

namespace ScyberLog
{
    internal class ColoredConsole : IDisposable
    {
        private ConsoleColor ForegroundColor { get; }
        private ConsoleColor BackgroundColor { get; }

        public ColoredConsole()
        {
            this.ForegroundColor = Console.ForegroundColor;
            this.BackgroundColor = Console.BackgroundColor;
        }

        public void Dispose()
        {
            Console.ForegroundColor = this.ForegroundColor;
            Console.BackgroundColor = this.BackgroundColor;
        }
    }
}