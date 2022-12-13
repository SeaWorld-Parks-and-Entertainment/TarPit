using System;
namespace SEA.DET.TarPit.Infrastructure.Redis;

public interface ICacheService
{
	int GetDifficulty(String caller);
	void CacheDifficulty(String caller, int difficulty);
	String GetHashes(String caller);
	void CacheHashes(String caller, String hashes);
	String GetTimestamps(String caller);
	void CacheTimestamps(String caller, String timestamps);
}
