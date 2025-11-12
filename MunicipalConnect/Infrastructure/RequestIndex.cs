using MunicipalConnect.Domain;
using MunicipalConnect.Infrastructure.Indexing;

namespace MunicipalConnect.Infrastructure
{
    public interface IRequestIndex
    {
        void Build(IEnumerable<ServiceRequest> seed);
        bool TryGet(string id, out ServiceRequest? r);
        IEnumerable<ServiceRequest> EnumerateSorted();
        IEnumerable<ServiceRequest> TopByPriority(int k);
    }

    public sealed class RequestIndex<TTree> : IRequestIndex
        where TTree : IOrderedIndex<string, ServiceRequest>, new()
    {
        private readonly TTree _tree = new();
        private readonly MinHeap<ServiceRequest> _heap =
            new((a, b) =>
            {
                int p = a.Priority.CompareTo(b.Priority);
                return p != 0 ? p : a.CreateAt.CompareTo(b.CreateAt);
            });

        public void Build(IEnumerable<ServiceRequest> seed)
        {
            foreach (var r in seed) { _tree.Upsert(r.TrackingId, r); _heap.Push(r); }
        }
        public bool TryGet(string id, out ServiceRequest? r) => _tree.TryGet(id, out r);
        public IEnumerable<ServiceRequest> EnumerateSorted() => _tree.InOrder().Select(x => x.Value);
        public IEnumerable<ServiceRequest> TopByPriority(int k)
        {
            var snap = new MinHeap<ServiceRequest>((x, y) =>
            {
                int p = x.Priority.CompareTo(y.Priority);
                return p != 0 ? p : x.CreateAt.CompareTo(y.CreateAt);
            });
            foreach (var x in _tree.InOrder().Select(t => t.Value)) snap.Push(x);
            for (int i = 0; i < k && snap.Count > 0; i++) yield return snap.Pop();
        }
    }
}
