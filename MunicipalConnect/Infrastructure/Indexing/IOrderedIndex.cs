using System;
using System.Collections.Generic;

namespace MunicipalConnect.Infrastructure.Indexing
{
    public interface IOrderedIndex<TKey, TValue> where TKey : IComparable<TKey>
    {
        void Upsert(TKey key, TValue value);
        bool TryGet(TKey key, out TValue value);
        IEnumerable<(TKey Key, TValue Value)> InOrder();
    }
}