using System;

namespace ScyberLog.Tests
{
    public class PropertyAccessException : Exception
    {
        public string BadProperty { get => throw new Exception("Exception accessing property"); }
        public PropertyAccessException(string message) : base(message) { }
    }
}