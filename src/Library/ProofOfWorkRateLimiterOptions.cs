using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace SEA.DET.TarPit.Library;

public class ProofOfWorkRateLimiterOptions : IProofOfWorkRateLimiterOptions
{
    public static String CallerIdentifierHeader = "caller-id";
    public Func<HttpContext, String> CallerIdentifier { get; set; }
    public static String SupplyIdentifierHeaderErrorMessage =
        $"Please supply a single {CallerIdentifierHeader} identifier header.";

    public String NonceHeader { get; }

    public String DifficultyHeader { get; }

    public ProofOfWorkRateLimiterOptions()
    {
        NonceHeader = "nonce";
        DifficultyHeader = "diff";
        CallerIdentifier = ((HttpContext httpContext) =>
        {
            if (!httpContext.Request.Headers.ContainsKey(CallerIdentifierHeader))
            {
                throw new Exception(SupplyIdentifierHeaderErrorMessage);
            }
            httpContext.Request.Headers.TryGetValue(
               CallerIdentifierHeader,
                out StringValues callerIdentifiers);
            if (callerIdentifiers.Count > 1)
            {
                throw new Exception(
                    $"Please only provide a single {CallerIdentifierHeader} caller identifier header.");
            }
            String? callerCandidate = callerIdentifiers[0];
            if (String.IsNullOrEmpty(callerCandidate))
            {
                throw new Exception(SupplyIdentifierHeaderErrorMessage);
            }
            return callerCandidate;
        });
    }

    //public ProofOfWorkRateLimiterOptions(
    //    Func<HttpContext, String> CallerIdentifier,
    //    String NonceHeader = "nonce",
    //    String DifficultyHeader = "difficulty")
    //{
    //    this.NonceHeader = NonceHeader;
    //    this.DifficultyHeader = DifficultyHeader;
    //    this.CallerIdentifier = CallerIdentifier;
    //}
}