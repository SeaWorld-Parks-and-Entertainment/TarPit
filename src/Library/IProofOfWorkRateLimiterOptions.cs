using System;
using Microsoft.AspNetCore.Http;

namespace SEA.DET.TarPit.Library;

public interface IProofOfWorkRateLimiterOptions
{
    String NonceHeader {get; }
    String DifficultyHeader { get; }
    Func<HttpContext, String> CallerIdentifier { get; }
}

