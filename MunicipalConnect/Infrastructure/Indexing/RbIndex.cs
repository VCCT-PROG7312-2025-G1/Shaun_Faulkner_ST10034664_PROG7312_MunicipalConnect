using System;
using System.Collections.Generic;

namespace MunicipalConnect.Infrastructure.Indexing
{
    ///------------------------------------
    /// <summary>
    /// Ordered map using red-black tree
    /// https://www.c-sharpcorner.com/article/working-with-red-black-trees-in-c-sharp/
    /// https://dzone.com/articles/red-black-trees-in-c-a-guide-to-efficient-self-bal
    /// https://www.geeksforgeeks.org/dsa/insertion-in-red-black-tree/
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    ///------------------------------------
    public sealed class RbIndex<TKey, TValue> : IOrderedIndex<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private enum C { R, B }

        ///------------------------------------
        /// Key = K, Value = V, N = Node, Left = L, Right = R, color = C, Parent = P
        ///------------------------------------
        private sealed class N
        {
            public TKey K;
            public TValue V;
            public N? L;
            public N? R;
            public N? P;
            public C Col;
            public N(TKey k, TValue v, C c) { K = k; V = v; Col = c; }
        }

        private N? _root;
        public int Count { get; private set; }

        public void Clear()
        {
            _root = null;
            Count = 0;
        }

        ///------------------------------------
        /// Insert or update
        ///------------------------------------
        public void Upsert(TKey k, TValue v)
        {
            if (k == null) throw new ArgumentNullException(nameof(k));

            N? y = null;
            var x = _root;
            while (x != null)
            {
                y = x;
                int c = k.CompareTo(x.K);
                if (c == 0)
                {
                    x.V = v;
                    return;
                }
                x = c < 0 ? x.L : x.R;
            }

            ///------------------------------------
            /// New red leaf
            ///------------------------------------

            var z = new N(k, v, C.R) { P = y };
            if (y == null)
            {
                _root = z;
                Count++;
            }
            else if (k.CompareTo(y.K) < 0)
            {
                y.L = z;
                Count++;
            }
            else
            {
                y.R = z;
                Count++;
            }

            FixIns(z);
        }

        ///------------------------------------
        /// O (log n) lookup
        ///------------------------------------
        public bool TryGet(TKey k, out TValue v)
        {
            v = default!;
            if (k == null) return false;

            var n = _root;
            while (n != null)
            {
                int c = k.CompareTo(n.K);
                if (c == 0) { v = n.V; return true; }
                n = c < 0 ? n.L : n.R;
            }
            return false;
        }

        public IEnumerable<(TKey Key, TValue Value)> InOrder()
        {
            var st = new Stack<N>();
            var cur = _root;
            while (cur != null || st.Count > 0)
            {
                while (cur != null) { st.Push(cur); cur = cur.L; }
                cur = st.Pop();
                yield return (cur.K, cur.V);
                cur = cur.R;
            }
        }

        ///------------------------------------
        /// Left rotation
        ///------------------------------------
        private void RotL(N x)
        {
            var y = x.R!;
            x.R = y.L;
            if (y.L != null) y.L.P = x;

            y.P = x.P;
            if (x.P == null) _root = y;
            else if (x == x.P.L) x.P.L = y;
            else x.P.R = y;

            y.L = x;
            x.P = y;
        }

        ///------------------------------------
        /// Right rotation
        ///------------------------------------
        private void RotR(N x)
        {
            var y = x.L!;
            x.L = y.R;
            if (y.R != null) y.R.P = x;

            y.P = x.P;
            if (x.P == null) _root = y;
            else if (x == x.P.R) x.P.R = y;
            else x.P.L = y;

            y.R = x;
            x.P = y;
        }

        ///------------------------------------
        /// Insert or fix up
        /// https://chatgpt.com/ - used to understand and debug insert and fix up
        ///------------------------------------
        private void FixIns(N z)
        {
            while (z.P != null && z.P.Col == C.R)
            {
                var gp = z.P.P;
                if (gp == null) break;

                if (z.P == gp.L)
                {
                    var y = gp.R;
                    if (y != null && y.Col == C.R)
                    {
                        z.P.Col = C.B;
                        y.Col = C.B;
                        gp.Col = C.R;
                        z = gp;
                    }
                    else
                    {
                        if (z == z.P.R)
                        {
                            z = z.P;
                            RotL(z);
                        }
                        z.P!.Col = C.B;
                        gp.Col = C.R;
                        RotR(gp);
                    }
                }
                else
                {
                    var y = gp.L;
                    if (y != null && y.Col == C.R)
                    {
                        z.P.Col = C.B;
                        y.Col = C.B;
                        gp.Col = C.R;
                        z = gp;
                    }
                    else
                    {
                        if (z == z.P.L)
                        {
                            z = z.P;
                            RotR(z);
                        }
                        z.P!.Col = C.B;
                        gp.Col = C.R;
                        RotL(gp);
                    }
                }
            }
            if (_root != null) _root.Col = C.B;
        }
    }
}