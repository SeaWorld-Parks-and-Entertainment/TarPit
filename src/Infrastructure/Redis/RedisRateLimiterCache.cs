using System;
using System.ComponentModel.Design;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure.Redis.Trie;

namespace SEA.DET.TarPit.Infrastructure.Redis;

public class RedisRateLimiterCache : IRateLimiterCache
{
    private readonly ICacheService _cacheService;

	public RedisRateLimiterCache(
        ICacheService cacheService)
	{
        _cacheService = cacheService;
	}

    public int GetDifficulty(string caller)
    {
        return _cacheService.GetDifficulty(caller: caller);
    }

    public void RecordNonce(string caller, string nonce, DateTimeOffset calledAt)
    {
        ITrieService trieService = new TrieService();
        String nonceTrieSerialized = _cacheService.GetHashes(caller: caller);
        trieService.WithSerializedTrie(nonceTrieSerialized);
        trieService.Insert(nonce);
        _cacheService.CacheHashes(
            caller: caller, hashes: trieService.SerializeTrie());
    }

    public bool SeenNonce(string caller, string nonce)
    {
        String nonceTrieSerialized = _cacheService.GetHashes(caller: caller);
        ITrieService trieService = new TrieService();
        trieService.WithSerializedTrie(nonceTrieSerialized);
        return trieService.Contains(nonce);
    }

    public int UpdateDifficulty(
        string caller, DateTimeOffset calledAt, int currentDifficulty,
        int requestsAllowedPerUnitTime, TimeSpan unitTime)
    {
        ITrieService trieService = new TrieService();
        String timestampTrieSerialized = _cacheService.GetTimestamps(caller: caller);
        trieService.WithSerializedTrie(timestampTrieSerialized);
        trieService.Insert(calledAt);
        _cacheService.CacheTimestamps(caller: caller, timestamps: trieService.SerializeTrie());

        DateTimeOffset leadingEdgeOfLookbackWindow = calledAt.Subtract(unitTime);

        int callsWithinLookbackWindow = trieService.NodesBetween(
            leadingEdgeOfLookbackWindow,
            calledAt.AddMilliseconds(1));

        if (callsWithinLookbackWindow > requestsAllowedPerUnitTime)
        {
            currentDifficulty++;
            _cacheService.CacheDifficulty(caller: caller, difficulty: currentDifficulty);
        }
        // TODO: Factor cooldown into library option.
        if (callsWithinLookbackWindow < (requestsAllowedPerUnitTime * 3 / 4))
        {
            if (currentDifficulty > 0)
            {
                currentDifficulty--;
                _cacheService.CacheDifficulty(caller: caller, difficulty: currentDifficulty);
            }
        }
        return currentDifficulty;
    }
}

