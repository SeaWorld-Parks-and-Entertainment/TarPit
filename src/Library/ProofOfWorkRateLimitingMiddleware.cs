using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure;
using SEA.DET.TarPit.Library;
using SEA.DET.TarPit.Library.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace SEA.DET.TarPit.Library;

public class ProofOfWorkRateLimitingMiddleware : IRateLimiter
{
    private readonly ICryptographicHasher _cryptographicHasher;
    private readonly IClock _clock;
    private readonly IRateLimiterCache _ratelimiterCache;

    private readonly String _nonceHeader = "nonce";
    private readonly String _difficultyHeader = "diff";
    private readonly Func<HttpContext, String> _callerIdentifier;

    private readonly int requestsAllowedPerUnitTime = 59;
    private readonly TimeSpan unitTime = TimeSpan.FromMinutes(1);


    public ProofOfWorkRateLimitingMiddleware(
        ICryptographicHasher cryptographicHasher,
        IClock clock,
        IRateLimiterCache ratelimiterCache,
        IProofOfWorkRateLimiterOptions rateLimiterOptions
        )
    {
        _cryptographicHasher = cryptographicHasher;
        _clock = clock;
        _ratelimiterCache = ratelimiterCache;
        _nonceHeader = rateLimiterOptions.NonceHeader;
        _callerIdentifier = rateLimiterOptions.CallerIdentifier;
        _difficultyHeader = rateLimiterOptions.DifficultyHeader;
    }

    public RateLimiterResponse RecordCallAndCalculateStatus(
        string caller, string nonce, DateTimeOffset calledAt)
    {
        // Proposed Sequence of Operations for a Relational DB:
        // Assumptions:
        // - TCP overhead kills.
        // Sequence:
        // * Transmit caller and nonce data to DB, returning difficulty.
        // * If nonce hash doesn't satisfy difficulty, return FAIL_DIFF.
        // * Query to see if we've the nonce before.
        // * If we've seen the nonce, return DUPLICATE_NONCE.
        // * Return OK.

        // The most efficient implementation would push literally all logic
        // into the DB, so we'll need to refactor into stored procedures once
        // the schema and queries are stable.

        // 1. Get current difficulty.
        // 2. Record call time and nonce.
        // 3. If current diff is zero, return OK.
        // 4. If we've seen the nonce, return DUPLICATE_NONCE.
        // 5. If nonce hash doesn't satisfy difficulty, return FAILS_DIFF.
        // 6. Return OK.
        int currentDifficulty = _ratelimiterCache.GetDifficulty(caller: caller);

        String hashedNonce = _cryptographicHasher.Hash(nonce);
        String proposedNullPrefix = hashedNonce.Substring(0, currentDifficulty);
        
        if (!proposedNullPrefix.All(s => s.Equals('0')))
        {
            // TODO: Return an HTTP response instead of throwing an exception.
            throw new InadequateDifficultyException(
                message: "Nonce does not hash to acceptable difficulty.",
                currentDifficulty: currentDifficulty);
        }
        if (_ratelimiterCache.SeenNonce(caller: caller, nonce: nonce))
        {
            // TODO: Return an HTTP response instead of throwing an exception.
            throw new ReusedNonceException(
                message: "This nonce has been used before. Please don't reuse nonces.",
                currentDifficulty: currentDifficulty);
        }

        _ratelimiterCache.RecordNonce(caller: caller, nonce: nonce, calledAt: calledAt);
        int updatedDifficulty = _ratelimiterCache.UpdateDifficulty(
            caller: caller, calledAt: calledAt,
            currentDifficulty: currentDifficulty,
            requestsAllowedPerUnitTime: requestsAllowedPerUnitTime,
            unitTime: unitTime);

        return new RateLimiterResponse(currentDifficulty: updatedDifficulty);
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        String nonce;
        context.Request.Headers.TryGetValue(_nonceHeader, out StringValues nonceValues);
        if (nonceValues.Count == 0) {
            nonce = "";
        } else
        {
            nonce = nonceValues[0] ?? "";
        }
        RateLimiterResponse rateLimiterResponse =
            RecordCallAndCalculateStatus
                (nonce: nonce,
                 caller: _callerIdentifier(context),
                 calledAt: _clock.Now);
        context.Response.Headers.Add
            (new KeyValuePair<String, StringValues>
                (_difficultyHeader,
                 new StringValues(rateLimiterResponse.CurrentDifficulty.ToString())));
        await next(context);
    }
}
