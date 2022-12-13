using System.Net;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure;
using SEA.DET.TarPit.Library;
using SEA.DET.TarPit.Library.IntegrationTests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using SEA.DET.TarPit.Test.Shared;
using Xunit;
using SEA.DET.TarPit.Library.Exceptions;

namespace SEA.DET.TarPit.Library.Tests;

public abstract class AbstractRatelimiterTests
{
    private IRateLimiter _rateLimiter;
    protected ITestRateLimiterCache _cacheService;
    protected MockClock _clock;
    protected List<(String, String)> NoncesAndHashes;
    public ProofOfWorkRateLimiterOptions rateLimiterOptions
        = new ProofOfWorkRateLimiterOptions();
    public IHost host;
    public HttpClient client;

    public AbstractRatelimiterTests(
        IRateLimiterFixture rateLimiterFixture)
    {
        _clock = rateLimiterFixture.Clock;
        _rateLimiter = rateLimiterFixture.RateLimiter;
        _cacheService = rateLimiterFixture.Cache;
        NoncesAndHashes = rateLimiterFixture.NoncesAndHashes;
    }

    [Fact]
    public void Test_RateLimitStatus_Sets_Timestamp_And_Nonce()
    {
        DateTimeOffset firstCallTime =
            new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1));
        _clock.SetCurrentTime(firstCallTime);
        String nonce = Guid.NewGuid().ToString();
        String caller = Guid.NewGuid().ToString();

        RateLimiterResponse rateLimiterResponse =
            _rateLimiter.RecordCallAndCalculateStatus(caller: caller, nonce: nonce,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));

        // TODO: factor these into tests.Infrastructure.CacheServiceUnitTests
        String? timestampTrieSerialized = _cacheService.GetTimestamps(caller);
        String? nonceTrieSerialized = _cacheService.GetHashes(caller);

        Assert.NotNull(timestampTrieSerialized);
        Assert.NotNull(nonceTrieSerialized);
    }

    [Fact]
    public void RateLimiter_Rejects_Nonces_That_Dont_Satisfy_Diff()
    {
        DateTimeOffset firstCallTime =
            new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1));
        _clock.SetCurrentTime(firstCallTime);
        String nonce = "AAAAA";
        String caller = Guid.NewGuid().ToString();
        _cacheService.CacheDifficulty(caller: caller, difficulty: 1);

        Assert.Throws<InadequateDifficultyException>(
            () =>
            {
                _rateLimiter.RecordCallAndCalculateStatus(
                    caller: caller, nonce: nonce,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));
            }
        );
    }

    [Fact]
    public void RateLimiter_Accepts_Nonces_That_Satisfy_Diff()
    {
        String nonce = NoncesAndHashes[0].Item1;
        String caller = Guid.NewGuid().ToString();
        _cacheService.CacheDifficulty(caller: caller, difficulty: 1);

        RateLimiterResponse rateLimiterResponse =
            _rateLimiter.RecordCallAndCalculateStatus(caller: caller, nonce: nonce,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));

        Assert.Equal(0, rateLimiterResponse.CurrentDifficulty);
    }

    [Fact]
    public void RateLimiter_Does_Not_Adjust_Diff_If_Threshold_Not_Exceeded()
    {
        String caller = Guid.NewGuid().ToString();
        RateLimiterResponse rateLimiterResponse = new RateLimiterResponse(
            currentDifficulty: Int32.MaxValue);

        foreach ((string, string) noncePair in NoncesAndHashes
            .Take(59))
        {
            rateLimiterResponse = _rateLimiter.RecordCallAndCalculateStatus(
                caller: caller, nonce: noncePair.Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));
        }

        Assert.Equal(0, rateLimiterResponse.CurrentDifficulty);
    }

    [Fact]
    public void RateLimiter_Refuses_Previously_Seen_Nonces()
    {
        String caller = Guid.NewGuid().ToString();

        _cacheService.CacheDifficulty(caller: caller, 1);
        _rateLimiter.RecordCallAndCalculateStatus(
            caller: caller, nonce: NoncesAndHashes[0].Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));
        _cacheService.CacheDifficulty(caller: caller, 1);
        Assert.Throws<ReusedNonceException>(
            () =>
            {
                _rateLimiter.RecordCallAndCalculateStatus(
                caller: caller, nonce: NoncesAndHashes[0].Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));
            });
    }

    [Fact]
    public void RateLimiter_Increases_Diff_If_Over_Threshold()
    {
        String caller = Guid.NewGuid().ToString();
        _clock.SetCurrentTime(new DateTimeOffset(
                new DateTime(year: 2022, month: 9, day: 11))
            );

        foreach ((string, string) noncePair in NoncesAndHashes
            .Take(59))
        {
            _rateLimiter.RecordCallAndCalculateStatus(
                caller: caller, nonce: noncePair.Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));
            _clock.Advance(TimeSpan.FromMilliseconds(10));
        }
        (string, string) triggeringNoncePair = NoncesAndHashes[59];

        RateLimiterResponse rateLimiterResponse =
            _rateLimiter.RecordCallAndCalculateStatus(
                caller: caller, nonce: triggeringNoncePair.Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));

        Assert.Equal(1, rateLimiterResponse.CurrentDifficulty);
    }

    [Fact]
    public void Ratelimiter_Reduces_Difficulty_When_Cooling_Down()
    {
        String caller = Guid.NewGuid().ToString();
        _clock.SetCurrentTime(new DateTimeOffset(
                new DateTime(year: 2022, month: 9, day: 11))
            );

        RateLimiterResponse rateLimiterResponse =
            new RateLimiterResponse(currentDifficulty: Int32.MaxValue);
        foreach ((string, string) noncePair in NoncesAndHashes
            .Take(60))
        {
            _clock.Advance(TimeSpan.FromSeconds(1));
            rateLimiterResponse = _rateLimiter.RecordCallAndCalculateStatus(
                caller: caller, nonce: noncePair.Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));
        }
        // Difficulty ratchets up after exceeding limits:
        Assert.Equal(1, rateLimiterResponse.CurrentDifficulty);

        (string, string) triggeringNoncePair = NoncesAndHashes[61];
        // This math actually makes zero sense to me, but I'm working on this in
        // recreation time, and not currently equipped to do the math right here.

        // Naively, we should advance the clock 16 seconds, and then 16 requests
        // should fall out of the lookback window, which is > 1/4 of the budget,
        // triggering a cooldown.

        // In this moment, I'm satisfied that cooldown is working, albeit
        // less quickly than it should.
        _clock.Advance(TimeSpan.FromSeconds(19)); 

        rateLimiterResponse =
            _rateLimiter.RecordCallAndCalculateStatus(
                caller: caller, nonce: triggeringNoncePair.Item1,
                calledAt: _clock.Now.Subtract(TimeSpan.FromMilliseconds(1)));

        Assert.Equal(0, rateLimiterResponse.CurrentDifficulty);
    }
    [Fact]
    public void Missing_Nonce_Header_Throws()
    {
        // TODO: Test actual error messages.
        Assert.ThrowsAsync<BadHttpRequestException>(
            () => client.GetAsync("/")
            );
    }

    private HttpClient getClient()
    {
        client.DefaultRequestHeaders.Add(
            ProofOfWorkRateLimiterOptions.CallerIdentifierHeader,
            "asdf");
        return client;
    }

    [Fact]
    public async void Middleware_Returns_Diff_Header()
    {

        HttpResponseMessage response = await getClient().GetAsync("/");

        response.Headers.TryGetValues(
            rateLimiterOptions.DifficultyHeader,
            out IEnumerable<string>? diffValue);
        if (diffValue == null)
        {
            Assert.Fail("diff should not be null");
        }
        Assert.Equal(0, int.Parse(diffValue.First()));
    }

    [Fact]
    public async void Middleware_Returns_Diff_Header_Updated_With_Current_Diff()
    {
        HttpClient _client = getClient();
        int i;
        for (i = 0; i < 59; i++)
        {
            _client.DefaultRequestHeaders.Remove
                (rateLimiterOptions.NonceHeader);
            _client.DefaultRequestHeaders.Add(
                rateLimiterOptions.NonceHeader, NoncesAndHashes[i].Item1);
            await _client.GetAsync("/");
            _clock.Advance(TimeSpan.FromMilliseconds(1));
        }
        _client.DefaultRequestHeaders.Remove(rateLimiterOptions.NonceHeader);
        _client.DefaultRequestHeaders.Add(
            rateLimiterOptions.NonceHeader, NoncesAndHashes[i].Item1);
        HttpResponseMessage responseMessage = await _client.GetAsync("/");
        responseMessage.Headers.TryGetValues(
            rateLimiterOptions.DifficultyHeader,
            out IEnumerable<string>? diffValue);
        Assert.Equal(1, int.Parse(diffValue.First()));
    }

    [Fact]
    public async void Present_Identifier_Header_Is_Handled()
    {
        client.DefaultRequestHeaders.Add(
            ProofOfWorkRateLimiterOptions.CallerIdentifierHeader,
            "asdf");
        HttpResponseMessage response = await client.GetAsync("/ok");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}