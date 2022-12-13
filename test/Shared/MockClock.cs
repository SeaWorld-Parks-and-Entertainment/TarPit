using System;
using SEA.DET.TarPit.Domain;

namespace SEA.DET.TarPit.Test.Shared;

public class MockClock : IClock
{
	DateTimeOffset _now;

	public MockClock(DateTimeOffset now)
	{
		_now = now;
	}

    public DateTimeOffset Now
	{
		get
		{
			return _now;
		}
	}

	public void SetCurrentTime(DateTimeOffset currentTime)
	{
		_now = currentTime;
	}

	public void Advance(TimeSpan timeSpan)
	{
		_now = _now.Add(timeSpan);
	}
}