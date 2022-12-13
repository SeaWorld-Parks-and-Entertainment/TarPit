using System;
namespace SEA.DET.TarPit.Library.Exceptions;

public class InadequateDifficultyException : RateLimiterException {

	    public InadequateDifficultyException() { }

    public InadequateDifficultyException(string message) : base(message) { }

    public InadequateDifficultyException(string message, int currentDifficulty) :
    base(message)
    {
        CurrentDifficulty = currentDifficulty;
    }

    public InadequateDifficultyException(string message, Exception inner) :
        base(message, inner)
    { }
}
