using System;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace SEA.DET.TarPit.Infrastructure.Redis.Trie;

public class TrieService : ITrieService
{
    TrieNode _rootNode;

    public TrieService()
    {
        _rootNode = new TrieNode();
    }

    public TrieService(TrieNode rootNode)
	{
        if (rootNode.Parent != null)
        {
            throw new ArgumentException(
                "TrieService must be instantiated with a root TrieNode.");
        }
        _rootNode = rootNode;
    }

    public void Insert(String toInsert)
    {
        Insert(toInsert, _rootNode);
    }

    public void Insert(DateTimeOffset toInsert)
    {
        Insert(toInsert: toInsert.ToUnixTimeMilliseconds().ToString());
    }

    private static void Insert(string toInsert, TrieNode parentNode)
    {
        if (!toInsert.Any())
        // Empty string, no more nodes to insert.
        // Mark this node to indicate that it terminates an inserted string.
        {
            parentNode.IsTerminal = true;
            return;
        }

        char firstCharOfInsertingValue = toInsert[0];

        if (!parentNode.Children.ContainsKey(firstCharOfInsertingValue))
        // Create this node, and add it into Children.
        {
            TrieNode newNode = new TrieNode(parent: parentNode);
            newNode.Value = firstCharOfInsertingValue;
            parentNode.Children[firstCharOfInsertingValue] = newNode;
        }

        // Insert the rest of the string in the node representing the first
        // character of this string.
        Insert(toInsert.Substring(1), parentNode.Children[firstCharOfInsertingValue]);
    }

    public bool Contains(String toFind)
    {
        if (Find(toFind: toFind) == null)
        {
            return false;
        }
        return true;
    }

    public bool Contains(DateTimeOffset toFind)
    {
        return Contains(toFind.ToUnixTimeMilliseconds().ToString());
    }

    public TrieNode? Find(String toFind)
    {
        return Find(toFind, _rootNode);
    }

    public TrieNode? Find(DateTimeOffset toFind)
    {
        return Find(toFind.ToUnixTimeMilliseconds().ToString());
    }

    private static TrieNode? Find(
        String toFind, TrieNode parentNode)
        // Not entirely certain this is the right return type.
    {
        if (!toFind.Any())
        // We've reached the node representing the end of the string that
        // we're looking for.
        {
            if (!parentNode.IsTerminal)
            // If this node is not terminal (SEA of SEAS or SEAS of SEAS):
            {
                // This node does not represent the string we sought.
                return null;
            }
            // We know that this node is terminal therefore terminates our
            // search with a positive result.
            return parentNode;
        }

        char firstCharOfSoughtString = toFind[0];

        if (!parentNode.Children.ContainsKey(firstCharOfSoughtString))
        {
            // This node does not have any children with the rest of the string, and so it does not contain the string to find.
            return null;
        }

        return Find(toFind.Substring(1), parentNode.Children[firstCharOfSoughtString]);
    }

    public int NodesBefore(String before)
    {
        return NodesBefore(before: before, parentNode: _rootNode);
    }

    public int NodesBefore(DateTimeOffset before)
    {
        return NodesBefore(
            before: before.ToUnixTimeMilliseconds().ToString(),
            parentNode: _rootNode);
    }

    /// <summary>
    /// Calculates the count of nodes before the provided string.
    /// </summary>
    /// <param name="before">String</param>
    /// <returns>int</returns>
    private static int NodesBefore(String before, TrieNode parentNode)
    {
        int retCount = 0;
        Stack<TrieNode> nodesToProcess =
            new Stack<TrieNode>();
        foreach (KeyValuePair<char, TrieNode>
            child in parentNode.Children)
        {
            nodesToProcess.Push(child.Value);
        }
        TrieNode? workingNode;
        while (nodesToProcess.TryPeek(out workingNode))
        {
            nodesToProcess.Pop();
            if (before.CompareTo(workingNode.NodeValue) <= 0)
            {
                break;
            }
            if (workingNode.IsTerminal)
            {
                retCount++;
            }

            foreach (KeyValuePair<char, TrieNode>
                child in workingNode.Children)
            {
                nodesToProcess.Push(child.Value);
            }
        }
        return retCount;
    }

    public int NodesBetween(String beginning, String end)
    {
        int nodesBeforeEnd = NodesBefore(end);
        int nodesBeforeBeginning = NodesBefore(beginning);

        return nodesBeforeEnd - nodesBeforeBeginning; 
    }

    public int NodesBetween(DateTimeOffset beginning, DateTimeOffset end)
    {
        return NodesBetween(
            beginning: beginning.ToUnixTimeMilliseconds().ToString(),
            end: end.ToUnixTimeMilliseconds().ToString());
    }

    public void WithSerializedTrie(string trieToInstantiate)
    {
         TrieNode parsedNodes = JsonConvert.DeserializeObject<TrieNode>(
            trieToInstantiate,
            new JsonSerializerSettings
            {
                MaxDepth = 256
            })
            ??
            new TrieNode();

        _rootNode = parsedNodes;
    }

    public String SerializeTrie()
    {
        return JsonConvert.SerializeObject(_rootNode);
    }
}

