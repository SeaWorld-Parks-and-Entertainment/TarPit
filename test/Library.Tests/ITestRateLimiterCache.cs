using System;
using SEA.DET.TarPit.Infrastructure;

namespace SEA.DET.TarPit.Library.Tests;

public interface ITestRateLimiterCache : IRateLimiterCache
{
    void CacheDifficulty(string caller, int difficulty);
    string? GetHashes(string caller);
    string? GetTimestamps(string caller);
}
