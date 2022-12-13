using System;
using System.Net;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure;
using SEA.DET.TarPit.Library.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NodaTime;
using SEA.DET.TarPit.Test.Shared;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Http;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Numerics;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Runtime.CompilerServices;
using Xunit.Sdk;
using SEA.DET.TarPit.Infrastructure.Redis;
using SEA.DET.TarPit.Infrastructure.Redis.Trie;

namespace SEA.DET.TarPit.Library.IntegrationTests;


public class RedisTestRateLimiterCache : RedisRateLimiterCache, ITestRateLimiterCache
{
    ICacheService _cacheService;
    public RedisTestRateLimiterCache(ICacheService cacheService) : base(cacheService)
    {
        _cacheService = cacheService;
    }

    public void CacheDifficulty(string caller, int difficulty)
    {
        _cacheService.CacheDifficulty(caller: caller, difficulty: difficulty);
    }

    public string? GetHashes(string caller)
    {
        return _cacheService.GetHashes(caller: caller);
    }

    public string? GetTimestamps(string caller)
    {
        return _cacheService.GetTimestamps(caller: caller);
    }
}

public class RedisRateLimiterFixture : RootFixture, IRateLimiterFixture
{
    
    public RedisRateLimiterFixture()
    {
        IDistributedCache distributedCache = new MemoryDistributedCache(
            Options.Create<MemoryDistributedCacheOptions>(
                new MemoryDistributedCacheOptions()));
        ICacheService _cacheService = new CacheService(distributedCache: distributedCache);
        ITrieService trieService = new TrieService();
        Cache = new RedisTestRateLimiterCache(cacheService: _cacheService);
        rateLimiterOptions = new ProofOfWorkRateLimiterOptions();
        RateLimiter = new ProofOfWorkRateLimitingMiddleware(
            cryptographicHasher: cryptographicHasher,
            clock: Clock,
            ratelimiterCache: Cache,
            rateLimiterOptions: rateLimiterOptions
        );
    }
}

public class RedisRateLimiterTests : AbstractRatelimiterTests, IClassFixture<RedisRateLimiterFixture>
{
    String caller = "asdf";

    public RedisRateLimiterTests(RedisRateLimiterFixture redisRateLimiterFixture) :
        base(redisRateLimiterFixture)
    {
        rateLimiterOptions = redisRateLimiterFixture.rateLimiterOptions;
        host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddDistributedMemoryCache();
                    services.AddProofOfWorkRateLimiting<ProofOfWorkRateLimiterOptions>();
                    services.AddMvc((options) =>
                    {
                        options.EnableEndpointRouting = false;

                    });
                })
                .Configure(app =>
                {
                    app.UseProofOfWorkRateLimitingMiddleware();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/ok", async context =>
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("ok");
                        });
                    });
                });
            })
            .Start();
        client = host.GetTestClient();
    }
}
