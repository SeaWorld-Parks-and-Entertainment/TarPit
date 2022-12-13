using System;
namespace SEA.DET.TarPit.Library;

public class RateLimiterResponse
{
	public int CurrentDifficulty { get; }
	public RateLimiterResponse(int currentDifficulty)
	{
		CurrentDifficulty = currentDifficulty;
	}
}
