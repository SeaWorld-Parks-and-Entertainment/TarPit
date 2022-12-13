using System;
using Microsoft.Extensions.Caching.Distributed;

namespace SEA.DET.TarPit.Infrastructure.Redis;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    private String TimestampTrieCacheKeyFormatter = "timestamps_{0}";
    private String NonceTrieCacheKeyFormatter = "nonces_{0}";
    private String CurrentDiffultyKeyFormatter = "diff_{0}";

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public void CacheDifficulty(string caller, int difficulty)
    {
        String cacheKey = String.Format(CurrentDiffultyKeyFormatter, caller);
        _distributedCache.SetStringAsync(cacheKey, difficulty.ToString());
    }

    public void CacheHashes(string caller, string hashes)
    {
        String cacheKey = String.Format(NonceTrieCacheKeyFormatter, caller);
        _distributedCache.SetStringAsync(cacheKey, hashes);
    }

    public void CacheTimestamps(string caller, string timestamps)
    {
        _distributedCache.SetStringAsync(
            String.Format(TimestampTrieCacheKeyFormatter, caller),
            timestamps);
    }

    public int GetDifficulty(string caller)
    {
        String cacheKey = String.Format(CurrentDiffultyKeyFormatter, caller);
        String? difficultyString = _distributedCache.GetString(cacheKey);
        if (difficultyString == null)
        {
            int difficulty = 0;
            difficultyString = difficulty.ToString();
            CacheDifficulty(caller: caller, difficulty: difficulty);
        }
        return int.Parse(difficultyString);
    }

    public string GetHashes(string caller)
    {
        return _distributedCache.GetString(
            String.Format(NonceTrieCacheKeyFormatter, caller)) ?? "";
    }

    public string GetTimestamps(string caller)
    {
        return _distributedCache.GetString(
            String.Format(TimestampTrieCacheKeyFormatter, caller)) ?? "";
    }
}

