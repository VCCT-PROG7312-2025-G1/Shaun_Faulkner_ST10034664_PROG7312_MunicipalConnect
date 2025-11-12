using System;
using System.Collections.Generic;

namespace MunicipalConnect.Infrastructure.Indexing
{

    ///------------------------------------
    /// <summary>
    /// Balanced ordered indexing with AVL tree
    /// https://www.geeksforgeeks.org/dsa/introduction-to-avl-tree/
    /// https://www.w3schools.com/dsa/dsa_data_avltrees.php
    /// https://dzone.com/articles/understanding-avl-trees-in-c-a-guide-to-self-balan
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// ///------------------------------------
    public sealed class AvlIndex<TKey, TValue> : IOrderedIndex<TKey, TValue>
        where TKey : IComparable<TKey>
    {

        ///------------------------------------
        /// Key = K, Value = V, Left = L, Right = R and a cached height | minimal node N
        ///------------------------------------
        private sealed class N
        {
            public TKey K;
            public TValue V;
            public N? L;
            public N? R;
            public int H = 1;
            public N(TKey k, TValue v) { K = k; V = v; }
        }

        private N? _root;

        public int Count { get; private set; }

        public void Clear() { _root = null; Count = 0; }

        public void Upsert(TKey k, TValue v)
        {
            if (k == null) throw new ArgumentNullException(nameof(k));
            _root = Ins(_root, k, v);
        }

        ///------------------------------------
        /// O (Log n) lookup
        ///------------------------------------
        public bool TryGet(TKey k, out TValue v)
        {
            v = default!;
            if (k == null) return false;

            var n = _root;
            while (n != null)
            {
                int c = k.CompareTo(n.K);
                if (c == 0)
                {
                    v = n.V;
                    return true;
                }
                n = c < 0 ? n.L : n.R;
            }
            return false;
        }

        public IEnumerable<(TKey, TValue)> InOrder()
        {
            var st = new Stack<N>();
            var cur = _root;

            while (cur != null || st.Count > 0)
            {
                while (cur != null)
                {
                    st.Push(cur);
                    cur = cur.L;
                }
                cur = st.Pop();
                yield return (cur.K, cur.V);
                cur = cur.R;
            }
        }

        private static int H(N? n) => n?.H ?? 0;
        private static void Fix(N n) => n.H = Math.Max(H(n.L), H(n.R)) + 1;
        private static int Bal(N n) => H(n.L) - H(n.R);

        ///------------------------------------
        /// Right rotation
        ///------------------------------------
        private static N RotR(N y)
        {
            var x = y.L!;
            var t = x.R;
            x.R = y;
            y.L = t;
            Fix(y);
            Fix(x);
            return x;
        }

        ///------------------------------------
        /// Left rotation
        ///------------------------------------
        private static N RotL(N x)
        {
            var y = x.R!;
            var t = y.L;
            y.L = x;
            x.R = t;
            Fix(x);
            Fix(y);
            return y;
        }

        ///------------------------------------
        /// Rebalance for AVL
        ///------------------------------------
        private N Ins(N? n, TKey k, TValue v)
        {
            if (n == null)
            {
                Count++;
                return new N(k, v);
            }

            int c = k.CompareTo(n.K);
            if (c < 0) n.L = Ins(n.L, k, v);
            else if (c > 0) n.R = Ins(n.R, k, v);
            else
            {
                n.V = v;
                return n;
            }

            Fix(n);
            int b = Bal(n);

            ///------------------------------------
            /// AVL cases
            ///------------------------------------
            
            if (b > 1 && k.CompareTo(n.L!.K) < 0) return RotR(n);
            if (b < -1 && k.CompareTo(n.R!.K) > 0) return RotL(n);
            if (b > 1 && k.CompareTo(n.L!.K) > 0)
            {
                n.L = RotL(n.L!);
                return RotR(n);
            }
            if (b < -1 && k.CompareTo(n.R!.K) < 0)
            {
                n.R = RotR(n.R!);
                return RotL(n);
            }

            return n;
        }
    }
}
