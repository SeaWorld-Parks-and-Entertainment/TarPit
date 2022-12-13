using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace SEA.DET.TarPit.Infrastructure.Redis.Trie;

[JsonObject(MemberSerialization.OptIn)]
public class TrieNode
{
    public TrieNode()
    {
        IsTerminal = false;
    }

    public TrieNode(TrieNode parent)
    {
        Parent = parent;
    }

    [JsonConstructor]
    public TrieNode(
        Dictionary<char, TrieNode> children)
    {
        Children = new SortedDictionary<char, TrieNode>(
            Comparer<char>.Create((x, y) => y.CompareTo(x))
        );
        foreach (KeyValuePair<char, TrieNode> entry in children)
        {
            entry.Value.Parent = this;
            entry.Value.Value = entry.Key;
            Children.Add(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// Child nodes. Library default is reverse ascii-betical, so we implement
    /// a Comparer that reverses the comparison order.
    /// </summary>
    [JsonProperty]
    public SortedDictionary<char, TrieNode>
        Children { get; set; } =
        new SortedDictionary<char, TrieNode>(
            Comparer<char>.Create((x, y) => y.CompareTo(x))
        );

    /// <summary>
    /// Node's parent. Null if root node.
    /// </summary>
    public TrieNode? Parent { get; set; }

    /// <summary>
    /// Is there an inserted string that terminates at this node?
    /// TODO: Consider renaming to TerminatesSubstring
    /// </summary>
    [JsonProperty]
    public bool IsTerminal { get; set; } = false;

    /// <summary>
    /// The value of this node of the tree. IsTerminal == false and
    /// Value == "" indicates the root
    /// </summary>
    public char Value { get; set; }

    /// <summary>
    /// The value represented by this node in the Trie.
    /// Could be time-optimized at the cost of space by caching the full value
    /// rather than the node-value on the node.
    /// </summary>
    /// <returns></returns>
    public string NodeValue
    {
        get
        {
            if (Parent == null)
            {
                return "";
            }
            if (String.Equals(Parent.Value, ""))
            {
                return Value.ToString();
            }
            return Parent.NodeValue + Value.ToString();
        }
    }
}
