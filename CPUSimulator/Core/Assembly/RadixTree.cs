namespace CPUSimulator.Core.Assembly
{
    using Hexa.NET.Utilities;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    public unsafe class RadixTree<T> where T : unmanaged
    {
        public unsafe struct Node
        {
            public byte* Key;
            public ushort KeyLength;
            public ushort Parent;
            public ChildrenArray<ushort> Children;
            public T Value;

            [InlineArray(Length)]
            public struct ChildrenArray<TIndex> where TIndex : unmanaged
            {
                public const int Length = 256;
                TIndex index;
            }

            public readonly Span<byte> KeySpan => new(Key, KeyLength);

            public Span<ushort> ChildrenSpan => MemoryMarshal.CreateSpan(ref Unsafe.As<ChildrenArray<ushort>, ushort>(ref Children), ChildrenArray<ushort>.Length);

            public readonly byte* KeyEnd => Key + KeyLength;
        }

        private Node* nodes;
        private ushort nodesCapacity;
        private ushort nodeCount;
        private StringPool pool;

        private const ushort RootIndex = 0;
        private const ushort InvalidIndex = ushort.MaxValue;

        public RadixTree()
        {
            CreateNode([], default, InvalidIndex);
        }

        public void Dispose()
        {
            Free(nodes);
            pool.Dispose();
        }

        private ushort CreateNode(ReadOnlySpan<byte> key, T value, ushort parent)
        {
            if (key.Length == 0 && nodeCount != 0)
            {
                throw new InvalidOperationException("The root node must have an empty key and no parent.");
            }
            var index = nodeCount++;
            if (nodes == null || nodeCount >= nodesCapacity)
            {
                var newCapacity = Math.Max(nodesCapacity * 2, 4);
                var newNodes = AllocT<Node>(newCapacity);
                if (nodes != null)
                {
                    MemcpyT(nodes, newNodes, nodesCapacity);
                    Free(nodes);
                }
                nodes = newNodes;
                nodesCapacity = (ushort)newCapacity;
            }
            Node* node = &nodes[index];
            node->Key = AllocateKey(key);
            node->KeyLength = (ushort)key.Length;
            node->Parent = parent;
            node->Value = value;
            node->ChildrenSpan.Fill(InvalidIndex);
            if (parent != InvalidIndex)
            {
                nodes[parent].Children[key[0]] = index;
            }
            return index;
        }

        private ushort SplitNode(ushort index, byte* split)
        {
            Node* node = &nodes[index];
            ushort newKeyLength = (ushort)(split - node->Key);
            ushort newIndex = CreateNode(new(node->Key, newKeyLength), default, node->Parent);
            node = &nodes[index];
            node->Parent = newIndex;
            node->KeyLength -= newKeyLength;
            node->Key = split;
            nodes[newIndex].Children[split[0]] = index;
            return newIndex;
        }

        private byte* AllocateKey(ReadOnlySpan<byte> key)
        {
            return pool.Take(key).Ptr;
        }

        public void Insert(ReadOnlySpan<byte> key, T value)
        {
            ushort currentIndex = RootIndex;

            while (true)
            {
                var commonPrefixLength = GetCommonPrefixLength(key, nodes[currentIndex].KeySpan);
                if (commonPrefixLength < nodes[currentIndex].KeyLength)
                {
                    ushort newIndex = SplitNode(currentIndex, nodes[currentIndex].Key + commonPrefixLength);
                    if (commonPrefixLength == key.Length)
                    {
                        nodes[newIndex].Value = value;
                        return;
                    }
                    CreateNode(key[commonPrefixLength..], value, newIndex);
                    return;
                }

                key = key[commonPrefixLength..];

                if (key.IsEmpty)
                {
                    nodes[currentIndex].Value = value;
                    return;
                }

                var parentIndex = currentIndex;
                currentIndex = nodes[currentIndex].Children[key[0]];

                if (currentIndex == InvalidIndex)
                {
                    CreateNode(key, value, parentIndex);
                    return;
                }
            }
        }

        public bool TryLookup(ReadOnlySpan<byte> key, out T value)
        {
            ushort currentIndex = RootIndex;

            while (currentIndex != InvalidIndex)
            {
                ref Node currentNode = ref nodes[currentIndex];
                var commonPrefixLength = GetCommonPrefixLength(key, currentNode.KeySpan);

                if (commonPrefixLength < currentNode.KeyLength)
                {
                    value = default;
                    return false;
                }

                key = key[commonPrefixLength..];

                if (key.IsEmpty)
                {
                    value = currentNode.Value;
                    return true;
                }

                currentIndex = currentNode.Children[key[0]];
            }

            value = default;
            return false;
        }

        public bool TryLookup(byte* key, int keyLength, out T value)
        {
            return TryLookup(new ReadOnlySpan<byte>(key, keyLength), out value);
        }

        public bool TryLookupLongestMatch(ReadOnlySpan<byte> key, out T value, out int matchLength)
        {
            ushort currentIndex = RootIndex;
            bool foundMatch = false;
            value = default;
            matchLength = 0;

            while (currentIndex != InvalidIndex)
            {
                ref Node currentNode = ref nodes[currentIndex];
                var commonPrefixLength = GetCommonPrefixLength(key, currentNode.KeySpan);

                if (commonPrefixLength < currentNode.KeyLength)
                {
                    return foundMatch;
                }

                matchLength += commonPrefixLength;
                key = key[commonPrefixLength..];

                if (key.IsEmpty || !EqualityComparer<T>.Default.Equals(currentNode.Value, default))
                {
                    value = currentNode.Value;
                    foundMatch = true;
                }

                if (key.IsEmpty)
                {
                    return foundMatch;
                }

                currentIndex = currentNode.Children[key[0]];
            }

            return foundMatch;
        }

        public bool TryLookupLongestMatch(byte* key, int keyLength, out T value, out int matchLength)
        {
            return TryLookupLongestMatch(new ReadOnlySpan<byte>(key, keyLength), out value, out matchLength);
        }

        public bool TryLookupLongestMatch(byte* key, byte* keyEnd, out T value, out nuint matchLength)
        {
            ushort currentIndex = nodes[RootIndex].Children[*key];
            ushort lastIndex = InvalidIndex;
            byte* currentKey = key;

            while (currentIndex != InvalidIndex)
            {
                ref Node currentNode = ref nodes[currentIndex];
                var commonPrefixLength = GetCommonPrefixLength(currentKey, keyEnd, currentNode.Key, currentNode.KeyEnd);

                if (commonPrefixLength < currentNode.KeyLength)
                {
                    break;
                }
                currentKey += commonPrefixLength;
                lastIndex = currentIndex;

                if (currentKey >= keyEnd)
                {
                    break;
                }

                currentIndex = currentNode.Children[*currentKey];
            }

            matchLength = (nuint)(currentKey - key);
            bool foundMatch = matchLength > 0;
            value = foundMatch ? nodes[lastIndex].Value : default;
            return foundMatch;
        }

        private static nuint GetCommonPrefixLength(byte* a, byte* aEnd, byte* b, byte* bEnd)
        {
            byte* start = a;
            while (a < aEnd && b < bEnd && *a++ == *b++) ;
            return (nuint)(a - start);
        }

        public T Lookup(ReadOnlySpan<byte> key)
        {
            if (TryLookup(key, out T value))
            {
                return value;
            }
            throw new KeyNotFoundException("The specified key was not found in the Radix Tree.");
        }

        private static int GetCommonPrefixLength(ReadOnlySpan<byte> key1, ReadOnlySpan<byte> key2)
        {
            int length = Math.Min(key1.Length, key2.Length);
            for (int i = 0; i < length; i++)
            {
                if (key1[i] != key2[i])
                {
                    return i;
                }
            }
            return length;
        }
    }
}