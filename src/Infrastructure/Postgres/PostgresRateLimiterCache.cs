using System;
using SEA.DET.TarPit.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace SEA.DET.TarPit.Infrastructure.Postgres;

public class PostgresRateLimiterCache : IRateLimiterCache
{
    RateLimiterContext _dbContext;

	public PostgresRateLimiterCache(RateLimiterContext dbContext)
	{
        _dbContext = dbContext;
	}

    public int GetDifficulty(string caller)
    {
        // TODO: Factor the default value into the EF.
        Caller? maybeOut = _dbContext.Callers.SingleOrDefault(
            c => c.ExternalIdentifier.Equals(caller));
        if (maybeOut == null)
        {
            return 0;
        };
        return maybeOut.Difficulty;
    }

    public void RecordNonce(string caller, string nonce, DateTimeOffset calledAt)
    {
        _dbContext.Database.ExecuteSqlRaw(
            @$"
with
new_caller as
  (insert into callers (external_identifier, difficulty) select '{caller}', 0
   where not exists (select id from callers where external_identifier = '{caller}')
   returning id),
existing_caller as
  (select id from callers where external_identifier = '{caller}'),
caller as
  (select id from new_caller
   union all
   select id from existing_caller)
insert into call_records (caller_id, called_at, nonce)
values
((select id from caller), '{calledAt}', '{nonce}')");
    }

    public bool SeenNonce(string caller, string nonce)
    {
        return _dbContext.CallRecords.Any(
            cr =>
            cr.Caller.ExternalIdentifier.Equals(caller)
            &&
            cr.Nonce.Equals(nonce));
    }

    public int UpdateDifficulty(string caller,
        DateTimeOffset calledAt,
        int currentDifficulty,
        int requestsAllowedPerUnitTime,
        TimeSpan unitTime)
    {
        int diffOut = currentDifficulty;
        Instant trailingEdgeOfLookbackWindow = Instant
            .FromUnixTimeMilliseconds(
                calledAt.ToUnixTimeMilliseconds())
            .Plus(Duration.FromMilliseconds(1)); // Add one millisecond to capture this current call.
        DateTimeOffset externalLeadingEdgeOfLookbackWindow =
            calledAt.Subtract(unitTime);
        Instant internalLeadingEdgeOfLookbackWindow =
            Instant.FromUnixTimeMilliseconds(
                externalLeadingEdgeOfLookbackWindow.ToUnixTimeMilliseconds());
        int callsInLookbackWindow =
            _dbContext.CallRecords
            .Where(
                cr =>
                    cr.Caller.ExternalIdentifier.Equals(caller)
                    &&
                    cr.CalledAt < trailingEdgeOfLookbackWindow
                    &&
                    cr.CalledAt >= internalLeadingEdgeOfLookbackWindow)
            .Count(); // TODO: Figure out how to get the DB to calculate if count > requestsAllowedPerUnitTime
        if (callsInLookbackWindow > requestsAllowedPerUnitTime)
        {
            diffOut++;
        }
        if (callsInLookbackWindow < (requestsAllowedPerUnitTime * 3 / 4) && currentDifficulty > 0)
        {
            diffOut--;
        }
        if (diffOut != currentDifficulty)
        {
            _dbContext.Database.ExecuteSql(
                $"update callers set difficulty = {diffOut} where external_identifier = {caller}");
        }
        return diffOut;
    }
}

