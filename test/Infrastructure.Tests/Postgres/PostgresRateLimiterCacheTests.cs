using System;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure.Postgres;
using SEA.DET.TarPit.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SEA.DET.TarPit.Test.Shared;
using IClock = SEA.DET.TarPit.Domain.IClock;

namespace Infrastructure.Tests.Postgres;

public class PostgresRateLimiterCacheTests
{
    PostgresRateLimiterCache rateLimiterCache;

    // Cache dependencies.
    RateLimiterContext dbContext;
    IClock clock;
    
    public PostgresRateLimiterCacheTests()
	{
        dbContext = new RateLimiterContext();
        rateLimiterCache = new PostgresRateLimiterCache(dbContext: dbContext);
        clock = new MockClock(new DateTimeOffset(DateTime.UtcNow));
	}

    [Fact]
    public void PG_RateLimiter_Instantiates()
    {
        Assert.True(true);
    }

    [Fact]
    public void Difficulty_Is_Zero_For_Unseen_Callers()
    {
        int actualDifficulty = rateLimiterCache.GetDifficulty("asdf");

        Assert.Equal(0, actualDifficulty);
    }

    [Fact]
    public void Updating_Difficulty_Creates_Caller_Row_If_None_Exists()
    {
        String callerIdentifier = Guid.NewGuid().ToString();
        String nonce = Guid.NewGuid().ToString();
        bool callerExists = dbContext.Callers.Any(
            c => c.ExternalIdentifier.Equals(callerIdentifier));
        Assert.False(callerExists);

        //rateLimiterCache.UpdateDifficulty(
        //    );
    }
}

