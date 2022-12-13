using System;
namespace SEA.DET.TarPit.Infrastructure;

public interface IRateLimiterCache
{
    int GetDifficulty(String caller);
    int UpdateDifficulty(
        String caller, DateTimeOffset calledAt, int currentDifficulty,
        int requestsAllowedPerUnitTime, TimeSpan unitTime);
    bool SeenNonce(String caller, String nonce);
    void RecordNonce(String caller, String nonce, DateTimeOffset calledAt);
}