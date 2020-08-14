using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easy.Huffman
{
    public class Tree<TValue> where TValue : struct, IComparable, IComparable<TValue>, IConvertible, IEquatable<TValue>, IFormattable
    {
        private Dictionary<TValue, (byte count, ulong code)> leafCodes;

        public Tree(Span<TValue> values, Span<ulong> counts)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != counts.Length)
                throw new ArgumentException($"Length of {nameof(values)}({values.Length}) is not equal to length of {nameof(counts)}({counts.Length}).");
        }

        public Tree(Span<TValue> data)
        {
            leafCodes = new Dictionary<TValue, (byte count, ulong code)>();
            IDictionary<TValue, ulong> pairs = new Dictionary<TValue, ulong>();

            foreach(var value in data)
            {
                if (pairs.ContainsKey(value))
                    pairs[value]++;
                else
                {
                    leafCodes[value] = (0, 0);
                    pairs[value] = 1;
                }
            }

            LeafCount = pairs.Count;
            OnlineSortedList<Node> list = MakeList(pairs);
            Leafs = list.ToArray();
            MakeTree(list);
        }

        public Node Root { get; private set; }

        public int LeafCount { get; private set; }

        public Node[] Leafs { get; private set; }

        public (byte count, ulong code) this[TValue index]
        {
            get
            {
                return leafCodes[index];
            }
        }

        private void MakeTree(OnlineSortedList<Node> list)
        {
            while(list.Count > 1)
            {
                Node element1 = list[0];
                Node element2 = list[1];

                Node newNode = new Node(new KeyValuePair<TValue, ulong>(default, element1.Pair.Value + element2.Pair.Value),
                    element1,
                    element2);

                list.Remove(element1);
                foreach(var key in element1.Keys)
                    AddCode(key, false);
                list.Remove(element2);

                foreach (var key in element2.Keys)
                    AddCode(key, true);

                list.Add(newNode);
            }
            Root = list[0]; 
        }

        private void AddCode(TValue value, bool right)
        {
            (byte count, ulong code) element = leafCodes[value];
            
            element.count++;
            element.code = (byte)((element.code << 1) | (right ? 1ul : 0ul));
            
            leafCodes[value] = element;
        }

        private OnlineSortedList<Node> MakeList(IEnumerable<KeyValuePair<TValue, ulong>> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));


            OnlineSortedList<Node> list = new OnlineSortedList<Node>();
            foreach (var value in values)
                list.Add(new Node(value));

            return list;
        }

        public class Node : IComparable<Node>
        {
            public readonly KeyValuePair<TValue, ulong> Pair;
            public readonly Node Left;
            public readonly Node Right;
            public readonly bool IsLeaf;
            public readonly TValue[] Keys;

            public Node(KeyValuePair<TValue, ulong> pair, Node left = null, Node right = null)
            {
                Pair = pair;
                Left = left;
                Right = right;
                IsLeaf = (left == null) && (right == null);
                Keys = GetKeys().ToArray();
            }

            public int CompareTo(Node other)
            {
                return Pair.Value.CompareTo(other.Pair.Value);
            }

            private IEnumerable<TValue> GetKeys()
            {
                if(IsLeaf)
                    yield return Pair.Key;

                if (Left != null)
                    foreach (TValue value in Left.Keys)
                        yield return value;

                if (Right != null)
                    foreach (TValue value in Right.Keys)
                        yield return value;
            }
        }
    }
}
