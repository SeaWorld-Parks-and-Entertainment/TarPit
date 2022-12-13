using System;

namespace SEA.DET.TarPit.Infrastructure.Redis.Trie;

public interface ITrieService
{
    void WithSerializedTrie(String trieToInstantiate);
    String SerializeTrie();
    void Insert(String toInsert);
    void Insert(DateTimeOffset toInsert);
    bool Contains(String toFind);
    bool Contains(DateTimeOffset toFind);
    TrieNode? Find(String toFind);
    TrieNode? Find(DateTimeOffset toFind);
    int NodesBefore(String toFind);
    int NodesBefore(DateTimeOffset toFind);
    int NodesBetween(String beginning, String end);
    int NodesBetween(DateTimeOffset beginning, DateTimeOffset end);
}