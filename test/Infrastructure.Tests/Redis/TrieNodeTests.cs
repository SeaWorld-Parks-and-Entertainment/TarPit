using System;
using SEA.DET.TarPit.Infrastructure.Redis.Trie;
using Newtonsoft.Json;
using Xunit;

namespace SEA.DET.TarPit.Infrastructure.Redis.Tests;

public class TrieNodeTests
{
	TrieNode _trieNode;
	TrieService _trieService;

	public TrieNodeTests()
	{
        _trieNode = new TrieNode();
		_trieService = new TrieService(_trieNode);
    }

	[Fact]
	public void TrieNodes_JsonSerialize()
	{
		_trieService.Insert("asdf");
		_trieService.Insert("asdg");
		_trieService.Insert("ben");
		String serializedValue = JsonConvert.SerializeObject(_trieNode);
		Console.Write(serializedValue);
	}
}

