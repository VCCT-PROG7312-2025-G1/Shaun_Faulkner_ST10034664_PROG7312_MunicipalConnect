using System;
using System.Collections.Generic;

namespace MunicipalConnect.Infrastructure.Indexing
{

    ///------------------------------------
    /// <summary>
    /// Binary min-heap over T that uses comparison
    /// https://www.geeksforgeeks.org/dsa/introduction-to-min-heap-data-structure/
    /// https://www.c-sharpcorner.com/article/data-structure-behind-max-and-min-heap/
    /// https://chatgpt.com/ - used to assist with debugging of Up / Down pushes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    ///------------------------------------
    public sealed class MinHeap<T>
    {
        private readonly List<T> _a = new();
        private readonly Comparison<T> _cmp;

        ///------------------------------------
        /// Comparison
        ///------------------------------------
        public MinHeap(Comparison<T> cmp)
        {
            _cmp = cmp ?? throw new ArgumentNullException(nameof(cmp));
        }

        public MinHeap(IComparer<T> comparer)
        {
            if (comparer is null) throw new ArgumentNullException(nameof(comparer));
            _cmp = comparer.Compare;
        }

        public int Count => _a.Count;
        public bool IsEmpty => _a.Count == 0;

        public void Clear() => _a.Clear();

        public void Push(T x)
        {
            _a.Add(x);
            Up(_a.Count - 1);
        }

        ///------------------------------------
        /// Append all, heapifies from bottom-up
        ///------------------------------------
        public void PushRange(IEnumerable<T> items)
        {
            if (items is null) return;
            foreach (var it in items) _a.Add(it);
            Heapify();
        }

        public T Pop()
        {
            if (_a.Count == 0) throw new InvalidOperationException("Heap is empty.");
            var t = _a[0];
            var last = _a[^1];
            _a.RemoveAt(_a.Count - 1);
            if (_a.Count > 0)
            {
                _a[0] = last;
                Down(0);
            }
            return t;
        }

        public bool TryPop(out T value)
        {
            if (_a.Count == 0) { value = default!; return false; }
            value = Pop();
            return true;
        }

        public T Peek()
        {
            if (_a.Count == 0) throw new InvalidOperationException("Heap is empty.");
            return _a[0];
        }

        public bool TryPeek(out T value)
        {
            if (_a.Count == 0) { value = default!; return false; }
            value = _a[0];
            return true;
        }

        private void Heapify()
        {
            for (int i = (_a.Count / 2) - 1; i >= 0; i--) Down(i);
        }

        private void Up(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (_cmp(_a[i], _a[p]) >= 0) break;
                (_a[i], _a[p]) = (_a[p], _a[i]);
                i = p;
            }
        }

        private void Down(int i)
        {
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, m = i;
                if (l < _a.Count && _cmp(_a[l], _a[m]) < 0) m = l;
                if (r < _a.Count && _cmp(_a[r], _a[m]) < 0) m = r;
                if (m == i) break;
                (_a[i], _a[m]) = (_a[m], _a[i]);
                i = m;
            }
        }
    }
}
