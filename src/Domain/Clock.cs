using System;
namespace SEA.DET.TarPit.Domain;

public class Clock : IClock
{
	public Clock()
	{
	}

    public DateTimeOffset Now
	{
		get
		{
			return DateTimeOffset.UtcNow;
		}
	}
}

