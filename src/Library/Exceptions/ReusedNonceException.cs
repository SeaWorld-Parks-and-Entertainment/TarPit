using System;
namespace SEA.DET.TarPit.Library.Exceptions
{
    public class ReusedNonceException : RateLimiterException
    {
        public ReusedNonceException() { }

        public ReusedNonceException(string message) : base(message) { }

        public ReusedNonceException(string message, int currentDifficulty) :
        base(message)
        {
            CurrentDifficulty = currentDifficulty;
        }

        public ReusedNonceException(string message, Exception inner) :
            base(message, inner)
        { }
    }
}

