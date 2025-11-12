using System;
using System.Collections.Generic;

namespace MunicipalConnect.Infrastructure.Indexing
{

    ///------------------------------------
    /// <summary>
    /// Simple ordered index / unbalanced using binary search tree
    /// https://www.w3schools.com/dsa/dsa_data_binarysearchtrees.php
    /// https://www.geeksforgeeks.org/dsa/binary-search-tree-data-structure/
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// ///------------------------------------
    public sealed class BstIndex<TKey, TValue> : IOrderedIndex<TKey, TValue>
        where TKey : IComparable<TKey>
    {

        ///------------------------------------
        /// Key = K, Value = V, Left = L, Right = R | minimal node Node 
        ///------------------------------------
        private sealed class Node
        {
            public TKey K;
            public TValue V;
            public Node? L;
            public Node? R;
            public Node(TKey k, TValue v) { K = k; V = v; }
        }

        private Node? _root;
        public int Count { get; private set; }

        public void Clear()
        {
            _root = null;
            Count = 0;
        }

        ///------------------------------------
        /// Insert or replace value for the key
        ///------------------------------------

        public void Upsert(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _root = Ins(_root, key, value);
        }

        ///------------------------------------
        /// O(h) lookup
        ///------------------------------------

        public bool TryGet(TKey key, out TValue value)
        {
            value = default!;
            if (key == null) return false;

            var n = _root;
            while (n != null)
            {
                int c = key.CompareTo(n.K);
                if (c == 0) { value = n.V; return true; }
                n = c < 0 ? n.L : n.R;
            }
            return false;
        }

        public IEnumerable<(TKey Key, TValue Value)> InOrder()
        {
            var st = new Stack<Node>();
            var cur = _root;

            while (cur != null || st.Count > 0)
            {
                while (cur != null) { st.Push(cur); cur = cur.L; }
                cur = st.Pop();
                yield return (cur.K, cur.V);
                cur = cur.R;
            }
        }

        private Node Ins(Node? n, TKey k, TValue v)
        {
            if (n == null)
            {
                Count++;
                return new Node(k, v);
            }

            int c = k.CompareTo(n.K);
            if (c < 0) n.L = Ins(n.L, k, v);
            else if (c > 0) n.R = Ins(n.R, k, v);
            else n.V = v;

            return n;
        }
    }
}