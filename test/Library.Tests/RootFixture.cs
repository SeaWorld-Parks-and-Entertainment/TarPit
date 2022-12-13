using System;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Test.Shared;

namespace SEA.DET.TarPit.Library.Tests;

public abstract class RootFixture : IHashFixture, IDisposable
{
    public List<(String, String)> NoncesAndHashes { get; }
        = new List<(string, string)>();
    public MockClock Clock { get; set; } = new MockClock(
        now: new DateTime(year: 2021, month: 9, day: 11));
    public ITestRateLimiterCache Cache { get; set; }
    public IRateLimiter RateLimiter { get; set; }
    public ICryptographicHasher cryptographicHasher = new SHA256CryptographicHasher();
    public ProofOfWorkRateLimiterOptions rateLimiterOptions;

    protected RootFixture() // Several of these values must get set in inheriting classes.
    {
        String path = Path.Combine(
            Directory.GetCurrentDirectory(), "0_diff_nonces.txt");
        String[] lines = System.IO.File.ReadAllLines(path);
        foreach (String line in lines)
        {
            String[] nonceAndHash = line.Split(' ');
            NoncesAndHashes.Add((nonceAndHash[0], nonceAndHash[1]));
        }
    }

    public void Dispose() { }
}
