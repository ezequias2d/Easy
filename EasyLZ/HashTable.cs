using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Easy
{
    public unsafe class HashTable
    {
        private readonly Stack<Node> cache;
        private class Node
        {
            public int position;
            public int generation;
            public Node next;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node(int position, int generation, Node next)
            {
                this.position = position;
                this.generation = generation;
                this.next = next;
            }
        }

        private readonly Node[] table;
        private readonly int size;
        private readonly int maxGenerationOffset;
        private int generation;
        private Node cacheNode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashTable(int size, int maxGenerationOffset)
        {
            table = new Node[size];
            cache = new Stack<Node>();
            this.size = size;
            this.maxGenerationOffset = maxGenerationOffset;
            generation = 0;
            cacheNode = null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Node GetOrCreateNode(in int position, in int generation, in Node next)
        {
            if (cache.Count > 0)
            {
                Node result = cache.Pop();
                result.position = position;
                result.generation = generation;
                result.next = next;
                return result;
            }
            return new Node(position, generation, next);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReturnNode(in Node node)
        {
            cache.Push(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in ReadOnlySpan<byte> src, in int position)
        {
            int hash = Hash(src, position);
            table[hash] = new Node(position, generation, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(in ReadOnlySpan<byte> src, in int position)
        {
            Node node;
            if (cacheNode == null)
            {
                int hash = Hash(src, position);
                node = table[hash];
            }
            else
            {
                node = cacheNode.next;
            }

            while(node != null && node.next != null && (generation - node.next.generation) > maxGenerationOffset)
                node.next = null;

            if (node != null)
            {
                cacheNode = node;
                return node.position;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            generation++;
            cacheNode = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Hash(in ReadOnlySpan<byte> src, in int position)
        {
            fixed (byte* pSrc = src)
            {
                return (*(ushort*)(pSrc + position));
            }
        }
    }
}
