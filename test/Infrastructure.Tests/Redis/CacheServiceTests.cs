using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SEA.DET.TarPit.Infrastructure.Redis;

namespace Infrastructure.Redis.Tests;

public class CacheServiceTests
{
	ICacheService _cacheService;
    Random _random = new Random();

	public CacheServiceTests()
	{
        IDistributedCache distributedCache = new MemoryDistributedCache(
            Options.Create<MemoryDistributedCacheOptions>(
                new MemoryDistributedCacheOptions()));
        _cacheService = new CacheService(distributedCache: distributedCache);
    }

    [Fact]
    public void Difficulty_Is_Zero_When_Not_Set()
    {
        int difficulty = _cacheService.GetDifficulty(caller: "asdf");

        Assert.Equal(0, difficulty);
    }

    [Fact]
    public void Difficulty_Is_Some_Number_When_Set()
    {
        int diff = _random.Next(256);

        _cacheService.CacheDifficulty(caller: "asdf", difficulty: diff);

        Assert.Equal(diff, _cacheService.GetDifficulty(caller: "asdf"));

    }
}

