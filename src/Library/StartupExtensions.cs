using System;
using SEA.DET.TarPit.Domain;
using SEA.DET.TarPit.Infrastructure;
using SEA.DET.TarPit.Infrastructure.Redis;
using SEA.DET.TarPit.Infrastructure.Redis.Trie;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SEA.DET.TarPit.Library;

public static class StartupExtensions
{
	public static IServiceCollection AddProofOfWorkRateLimiting<T>(
		this IServiceCollection services)
		where T : class, IProofOfWorkRateLimiterOptions
	{
		services.AddSingleton<ICryptographicHasher, SHA256CryptographicHasher>();
		services.AddSingleton<ICacheService, CacheService>();
		services.AddSingleton<IRateLimiterCache, RedisRateLimiterCache>();
		services.AddSingleton<IClock, Clock>();
		services.AddSingleton<ITrieService, TrieService>();
		services.AddSingleton<IProofOfWorkRateLimiterOptions, T>();
		services.AddSingleton<IRateLimiter, ProofOfWorkRateLimitingMiddleware>();
		return services;
	}

	public static IApplicationBuilder UseProofOfWorkRateLimitingMiddleware(
		this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<IRateLimiter>();
	}
}