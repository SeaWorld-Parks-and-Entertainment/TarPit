using System;
using System.Data;
using System.Data.Common;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure.Postgres;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SEA.DET.TarPit.Library.Tests;
using SEA.DET.TarPit.Library;

namespace Library.Tests;

public class PostgresTestRateLimiterCache : PostgresRateLimiterCache, ITestRateLimiterCache
{
    private RateLimiterContext _dbContext;
    private IDbContextTransaction _dbTransaction;
    public PostgresTestRateLimiterCache(RateLimiterContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
        _dbTransaction = _dbContext.Database.BeginTransaction();
    }

    public void CacheDifficulty(string caller, int difficulty)
    {
        _dbContext.Database.ExecuteSql(
            @$"
insert into callers (external_identifier, difficulty) select {caller}, {difficulty}
on conflict(external_identifier) do update set difficulty=EXCLUDED.difficulty
");
    }

    public string? GetHashes(string caller)
    {
        return _dbContext.CallRecords.Single(
            callRecord =>
            callRecord.Caller.ExternalIdentifier.Equals(caller)).ToString();
    }

    public string? GetTimestamps(string caller)
    {
        return _dbContext.CallRecords.Single(
            callRecord =>
            callRecord.Caller.ExternalIdentifier.Equals(caller)).ToString();
    }

    public void Dispose()
    {
        _dbTransaction.Rollback();
    }
}

public class TestPostgresRateLimiterOptions : ProofOfWorkRateLimiterOptions
{
    new String NonceHeader = "nonce";
    new String DifficultyHeader = "difficulty";
    public static new String CallerIdentifierHeader = "caller-id";

    new Func<HttpContext, String> CallerIdentifier = ((HttpContext httpContext) => // TODO: Unnecessary?
    {
        httpContext.Request.Headers.TryGetValue(
           CallerIdentifierHeader,
            out StringValues callerIdentifiers);
        if (callerIdentifiers.Count > 1)
        {
            throw new BadHttpRequestException(
                $"Please only provide a single {CallerIdentifierHeader} caller identifier header.");
        }
        String? callerCandidate = callerIdentifiers[0];
        if (String.IsNullOrEmpty(callerCandidate))
        {
            throw new BadHttpRequestException(
                $"Please supply a single {CallerIdentifierHeader} identifier header.");
        }
        return callerCandidate;
    });
}


public class PostgresRateLimiterFixture : RootFixture, IRateLimiterFixture
{

	public PostgresRateLimiterFixture()
	{
		RateLimiterContext _dbContext = new RateLimiterContext();
		Cache = new PostgresTestRateLimiterCache(dbContext: _dbContext);
        rateLimiterOptions = new ProofOfWorkRateLimiterOptions();
        RateLimiter = new ProofOfWorkRateLimitingMiddleware(
            cryptographicHasher: cryptographicHasher,
            clock: Clock,
            ratelimiterCache: Cache,
            rateLimiterOptions: rateLimiterOptions);
	}
}

public class PostgresRateLimiterTests : AbstractRatelimiterTests, IClassFixture<PostgresRateLimiterFixture>
{
    public PostgresRateLimiterTests(PostgresRateLimiterFixture rateLimiterFixture) :
		base(rateLimiterFixture) {
        host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<RateLimiterContext>(
                        options =>
                        {
                            options.UseNpgsql("host=localhost;database=tarpit;user=tarpit");
                        });
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