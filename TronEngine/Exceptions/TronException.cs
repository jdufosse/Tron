using System;

namespace TronEngine.Exceptions
{
    [Serializable]
    public class TronException : NullReferenceException
    {
        public TronException() : base() { }

        public TronException(string message) : base(message) { }
    }
}
