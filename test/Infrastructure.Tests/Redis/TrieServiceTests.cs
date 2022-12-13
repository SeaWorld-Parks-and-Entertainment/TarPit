using System;
using SEA.DET.TarPit.Infrastructure.Redis.Trie;
using Xunit;

namespace SEA.DET.TarPit.Domain.UnitTests;

public class TrieServiceTests
{
	private TrieService _trieService;

	public TrieServiceTests()
	{
		TrieNode _trieNode = new TrieNode();
		_trieService = new TrieService(_trieNode);
    }

    [Fact]
    public void Empty_Trie_Doesnt_Contain_Empty_String()
    {
        bool contains = _trieService.Contains("");
        Assert.False(contains);
    }

    [Fact]
    public void Empty_Trie_Contains_Nothing()
    {
        bool contains = _trieService.Contains("asdfpqr");
        Assert.False(contains);
    }

    [Fact]
    public void Trie_With_Single_Node_Contains_That_String()
    {
        _trieService.Insert("a");

        bool contains = _trieService.Contains("a");

        Assert.True(contains);
    }

    [Fact]
    public void Trie_With_Single_Node_Only_Contains_That_String()
    {
        _trieService.Insert("a");

        bool contains = _trieService.Contains("B");

        Assert.False(contains);
    }

    [Fact]
    public void Trie_With_Multiple_Node_String_Contains_That_String()
    {
        _trieService.Insert("aa");

        bool contains = _trieService.Contains("aa");

        Assert.True(contains);
    }

    [Fact]
    public void Trie_With_Long_String_Doesnt_Contain_Unterminating_Substring()
    {
        _trieService.Insert("aa");

        bool contains = _trieService.Contains("a");

        Assert.False(contains);
    }

    [Fact]
    public void Trie_With_Long_String_Contains_Terminated_Substring()
    {
        _trieService.Insert("aa");
        _trieService.Insert("a");

        bool contains = _trieService.Contains("a");

        Assert.True(contains);
    }

    [Fact]
    public void Trie_With_Lon_String_Contains_Only_Itself()
    {
        _trieService.Insert("abcdefghijklmnopqrstuv");

        bool contains = _trieService.Contains("abcdefghijklmnopqrstuv");

        Assert.True(contains);

        bool containsSubstring = _trieService.Contains("abcdefghijklmnopqrstu");

        Assert.False(containsSubstring);

        bool containsMidspaceSubstring = _trieService.Contains("bcdefghijklmnopqrstuv");

        Assert.False(containsMidspaceSubstring);
    }

    [Fact]
    public void Can_Find_Specific_Node()
    {
        String testString = "abcdefghijk";
        _trieService.Insert(testString);
        TrieNode? created = (TrieNode?)_trieService.Find(toFind: testString);

        Assert.NotNull(created);
        Assert.Equal("k", created.Value.ToString());
    }

    [Fact]
    public void Nodes_Reconstruct_Their_Values()
    {
        String testString = "abcdefghijkl";
        _trieService.Insert(testString);
        TrieNode? created = (TrieNode?)_trieService.Find(toFind: testString);

        Assert.NotNull(created);
        Assert.Equal(testString, created.NodeValue);
    }

    [Fact]
    public void There_Are_No_Nodes_Before_The_Only_Node()
    {
        _trieService.Insert("a");

        Assert.Equal(0, _trieService.NodesBefore("a"));
    }

    [Fact]
    public void Can_Count_One_Node_Before()
    {
        _trieService.Insert("a");
        _trieService.Insert("b");

        Assert.Equal(1, _trieService.NodesBefore("b"));
    }

    [Fact]
    public void Tries_Can_Count_Nodes_Before_With_Permutations()
    {
        _trieService.Insert("a");
        _trieService.Insert("b");
        _trieService.Insert("c");
        _trieService.Insert("d");

        Assert.Equal(0, _trieService.NodesBefore("a"));
        Assert.Equal(1, _trieService.NodesBefore("b"));
        Assert.Equal(2, _trieService.NodesBefore("c"));
        Assert.Equal(3, _trieService.NodesBefore("d"));
    }

    [Fact]
    public void Tries_Count_Distinct_Nested_Nodes()
    {
        _trieService.Insert("a");
        _trieService.Insert("aa");
        _trieService.Insert("b");

        Assert.Equal(2, _trieService.NodesBefore("b"));
    }

    [Fact]
    public void Tries_Only_Count_Terminated_Strings()
    {
        _trieService.Insert("aa");
        _trieService.Insert("b");

        Assert.Equal(1, _trieService.NodesBefore("b"));
    }

    [Fact]
    public void Tries_Can_Count_Nodes_Before_Values_Not_In_The_Trie()
    {
        _trieService.Insert("a");

        Assert.Equal(1, _trieService.NodesBefore("b"));
    }

    [Fact]
    public void Tries_Dont_Count_Nodes_After_The_Supplied_Before_Parameter()
    {
        _trieService.Insert("a");
        _trieService.Insert("c");

        Assert.Equal(1, _trieService.NodesBefore("b"));
    }

    [Fact]
    public void Tries_Count_Nested_Nodes_And_Exclude_Nodes_After_Supplied_Parameter()
    {
        _trieService.Insert("a");
        _trieService.Insert("aa");
        _trieService.Insert("aaa");
        _trieService.Insert("aaaa");

        Assert.Equal(1, _trieService.NodesBefore("aa"));
        Assert.Equal(2, _trieService.NodesBefore("aaa"));
        Assert.Equal(3, _trieService.NodesBefore("aaaa"));
        Assert.Equal(4, _trieService.NodesBefore("aaaaa"));
        Assert.Equal(4, _trieService.NodesBefore("b"));
    }

    [Fact]
    public void Tries_Count_Nodes_Between_Nodes() // Timestamps, for example.
    {
        DateTime rootDateTime =
                new DateTime(
                    year: 2000, month: 1, day: 1, hour: 1, minute: 1, second: 0
                );
        for (int idx = 0; idx < 180; idx++)
        {
            DateTimeOffset toInsert =
                new DateTimeOffset(
                    rootDateTime.AddSeconds(idx));
            _trieService.Insert(toInsert);
        }
        DateTimeOffset beginningOfRequests = new DateTimeOffset(
            new DateTime(
                year: 2000, month: 1, day: 1, hour: 1, minute: 1, second: 0
            )
        );
        DateTimeOffset beginningOfWindow = new DateTimeOffset(
            new DateTime(
                year: 2000, month: 1, day: 1, hour: 1, minute: 2, second: 0
            )
        );

        DateTimeOffset endOfWindow = new DateTimeOffset(
            new DateTime(
                year: 2000, month: 1, day: 1, hour: 1, minute: 3, second: 0
            )
        );

        DateTimeOffset endOfRequests = new DateTimeOffset(
            new DateTime(
                year: 2000, month: 1, day: 1, hour: 1, minute: 4, second: 0
            )
        );

        Assert.Equal(
            60,
            _trieService.NodesBetween(
                beginning: beginningOfWindow,
                end: endOfWindow));
        Assert.Equal(
            60,
            _trieService.NodesBetween(
                beginning: beginningOfRequests,
                end: beginningOfWindow));
        Assert.Equal(
            120,
            _trieService.NodesBetween(
                beginning: beginningOfRequests,
                end: endOfWindow));
        Assert.Equal(
            180,
            _trieService.NodesBetween(
                beginning: beginningOfRequests,
                end: endOfRequests));
    }

    [Fact]
    public void Nodes_Are_Not_Before_Themselves()
    {
        String node1 = "aaa";

        _trieService.Insert(node1);

        Assert.Equal(
            0,
            _trieService.NodesBefore(node1));
    }

    [Fact]
    public void Nodes_Are_After_Predecessor_Terminal_Nodes()
    {
        String node1 = "aaa";
        String node2 = "aa";

        _trieService.Insert(node1);
        _trieService.Insert(node2);

        Assert.Equal(
            1,
            _trieService.NodesBefore(node1));
    }

    [Fact]
    public void TrieService_Counts_Accurately_With_Mixed_Terminal_And_Not_Nodes()
    {
        String node5 = "aa";
        String node1 = "aaa";
        String node4 = "aaabbbcd";
        String node2 = "bbb";
        String node3 = "bbcdd";

        _trieService.Insert(node1);
        _trieService.Insert(node2);
        _trieService.Insert(node3);
        _trieService.Insert(node4);
        _trieService.Insert(node5);

        Assert.Equal(5, _trieService.NodesBefore("z"));
        Assert.Equal(4, _trieService.NodesBefore(node3));
        Assert.Equal(3, _trieService.NodesBefore(node2));
        Assert.Equal(2, _trieService.NodesBefore(node4));
        Assert.Equal(1, _trieService.NodesBefore(node1));
    }

    [Fact]
    public void Elementary_Serialization_Round_Trip()
    {
        String node1 = "a";
        String node2 = "b";

        _trieService.Insert(node1);
        _trieService.Insert(node2);

        String trieSerialized = _trieService.SerializeTrie();
        _trieService.WithSerializedTrie(trieSerialized);
        
        Assert.Equal(
            0,
            _trieService.NodesBefore(node1));
        Assert.Equal(
            1,
            _trieService.NodesBefore(node2));
    }

    [Fact]
    public void TrieService_Serialization_Round_Trips()
    {
        String node5 = "aa";
        String node1 = "aaa";
        String node4 = "aaabbbcd";
        String node2 = "bbb";
        String node3 = "bbcdd";

        _trieService.Insert(node1);
        _trieService.Insert(node2);
        _trieService.Insert(node3);
        _trieService.Insert(node4);
        _trieService.Insert(node5);

        TrieService comparativeTrieService = new TrieService();
        comparativeTrieService.Insert(node1);
        comparativeTrieService.Insert(node2);
        comparativeTrieService.Insert(node3);
        comparativeTrieService.Insert(node4);
        comparativeTrieService.Insert(node5);

        String trieSerialized = _trieService.SerializeTrie();
        _trieService.WithSerializedTrie(trieSerialized);

        Assert.True(_trieService.Contains(node1));
        Assert.True(_trieService.Contains(node2));
        Assert.True(_trieService.Contains(node3));
        Assert.True(_trieService.Contains(node4));

        Assert.Equal(5, _trieService.NodesBefore("z"));
        Assert.Equal(4, _trieService.NodesBefore(node3));
        Assert.Equal(3, _trieService.NodesBefore(node2));
        Assert.Equal(2, _trieService.NodesBefore(node4));
        Assert.Equal(1, _trieService.NodesBefore(node1));

    }

    [Fact]
    public void TrieService_Counts_Timestamps()
    {
        DateTimeOffset rootDateTime =
            new DateTimeOffset(
                new DateTime(year: 2001, month: 9, day: 11));
        int nodeCountToInsert = 10;
        foreach (int i in Enumerable.Range(0, nodeCountToInsert))
        {
            DateTimeOffset tempOffset = rootDateTime.Add(TimeSpan.FromSeconds(i));
            _trieService.Insert(tempOffset);
        }

        Assert.Equal(
            10,
            _trieService.NodesBefore(
                rootDateTime.Add(TimeSpan.FromDays(1))));

        Assert.Equal(
            10,
            _trieService.NodesBetween(
                rootDateTime,
                rootDateTime.Add(TimeSpan.FromSeconds(nodeCountToInsert))
                    
                ));

        Assert.Equal(
            5,
            _trieService.NodesBetween(
                rootDateTime.Add(TimeSpan.FromSeconds(5))
                    ,
                rootDateTime.Add(TimeSpan.FromSeconds(nodeCountToInsert))
                    ));

        Assert.Equal(
            2,
            _trieService.NodesBetween(
                rootDateTime.Add(TimeSpan.FromSeconds(8))
                    ,
                rootDateTime.Add(TimeSpan.FromSeconds(nodeCountToInsert))
                    ));
    }

    [Fact]
    public void MoreTrieService_TimestampTests()
    {
        DateTimeOffset rootDateTime = new DateTimeOffset(
                new DateTime(year: 2001, month: 9, day: 11));
        DateTimeOffset afterCalls = new DateTimeOffset(
                new DateTime(year: 2022, month: 9, day: 11));

        DateTimeOffset toInsert = new DateTimeOffset(
                new DateTime(year: 2001, month: 9, day: 12));

        _trieService.Insert(
            toInsert);

        Assert.Equal(
            0,
            _trieService.NodesBefore(
                rootDateTime));
        Assert.Equal(
            1,
            _trieService.NodesBefore(
                rootDateTime.AddDays(2)));
        Assert.Equal(
            1,
            _trieService.NodesBefore(afterCalls));

        Assert.Equal(
            1,
            _trieService.NodesBetween(
                rootDateTime,
                afterCalls));
    }

    [Fact]
    public void Inscrutably_Failing_Between_Tests() {
        // Failing because we don't zero-pad unix timestrings!
        DateTimeOffset beforeCalls = new DateTimeOffset(
                new DateTime(year: 2001, month: 9, day: 11));
        DateTimeOffset toInsert = new DateTimeOffset(
                new DateTime(year: 2001, month: 9, day: 12));
        DateTimeOffset afterCalls = new DateTimeOffset(
                new DateTime(year: 2022, month: 9, day: 11));

        Assert.Equal(
            0,
            _trieService.NodesBefore(beforeCalls));
        Assert.Equal(
            0,
            _trieService.NodesBefore(afterCalls));

        _trieService.Insert(
            toInsert);

        TrieNode? trieNode = _trieService.Find(
            toFind: toInsert
            );

        Assert.Equal(
            0,
            _trieService.NodesBefore(beforeCalls));

        Assert.Equal(
            1,
            _trieService.NodesBefore(afterCalls));
    }

    [Fact]
    public void Reproduce_Lookback_Failures()
    {
        DateTimeOffset callsStart = new DateTimeOffset(
                new DateTime(year: 2022, month: 9, day: 11));
        DateTimeOffset leadingEdgeOfLookbackWindow =
            new DateTimeOffset(
                new DateTime(year: 2021, month: 9, day: 10));
        DateTimeOffset callsEnd = new DateTimeOffset(
                new DateTime(year: 2022, month: 9, day: 13));

        _trieService.Insert(
            callsStart);

        String trieSerialized = _trieService.SerializeTrie();
        _trieService.WithSerializedTrie(trieSerialized);

        Assert.Equal(1,
            _trieService.NodesBefore(
                callsEnd));
        Assert.Equal(
            0,
            _trieService.NodesBefore(leadingEdgeOfLookbackWindow));
    }
}

