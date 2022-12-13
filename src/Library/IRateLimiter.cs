using System;
using Microsoft.AspNetCore.Http;

namespace SEA.DET.TarPit.Library;

public interface IRateLimiter : IMiddleware
{
    RateLimiterResponse RecordCallAndCalculateStatus(
        String caller, String nonce, DateTimeOffset calledAt);
}

