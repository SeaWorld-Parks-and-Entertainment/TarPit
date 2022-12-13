using System;
namespace SEA.DET.TarPit.Library.Exceptions;

public class RateLimiterException : Exception
{
	public int CurrentDifficulty { get; set; }

	public RateLimiterException() { }

	public RateLimiterException(string message) : base(message) { }

	public RateLimiterException(string message, int currentDifficulty) :
		base(message)
	{
		CurrentDifficulty = currentDifficulty;
	}

	public RateLimiterException(string message, Exception inner) :
		base(message, inner) { }
}

