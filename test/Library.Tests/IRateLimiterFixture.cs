using System;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NodaTime;
using SEA.DET.TarPit.Test.Shared;

namespace SEA.DET.TarPit.Library.Tests;

public interface IHashFixture
{
    List<(String, String)> NoncesAndHashes { get; }
}

public interface IRateLimiterFixture : IHashFixture
{
	MockClock Clock { get; set; }
	ITestRateLimiterCache Cache { get; set; }
    IRateLimiter RateLimiter { get; set; }
}
