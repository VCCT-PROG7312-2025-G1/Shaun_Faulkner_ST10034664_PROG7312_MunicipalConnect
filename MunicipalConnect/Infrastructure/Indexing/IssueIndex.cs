using MunicipalConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalConnect.Infrastructure.Indexing
{
    ///------------------------------------
    /// <summary>
    /// Index for IssueReport
    /// </summary>
    ///------------------------------------
    public interface IIssueIndex
    {
        void Build(IEnumerable<IssueReport> seed);
        void Upsert(IssueReport issue);
        bool TryGetByTrackingId(string trackingId, out IssueReport issue);
        IEnumerable<IssueReport> AllSortedByTrackingId();
        IEnumerable<IssueReport> TopUrgent(int k);
    }

    ///------------------------------------
    /// TTree is any ordered index
    ///------------------------------------
    public sealed class IssueIndex<TTree> : IIssueIndex
        where TTree : IOrderedIndex<string, IssueReport>, new()
    {
        private TTree _byTracking = new();

        public void Build(IEnumerable<IssueReport> seed)
        {
            _byTracking = new TTree();

            foreach (var r in seed ?? Enumerable.Empty<IssueReport>())
            {
                if (r is null) continue;
                if (string.IsNullOrWhiteSpace(r.TrackingId)) continue;

                _byTracking.Upsert(r.TrackingId, r);
            }
        }

        public void Upsert(IssueReport issue)
        {
            if (issue is null) return;
            if (string.IsNullOrWhiteSpace(issue.TrackingId)) return;

            _byTracking.Upsert(issue.TrackingId, issue);
        }

        public bool TryGetByTrackingId(string trackingId, out IssueReport issue)
        {
            issue = default!;
            if (string.IsNullOrWhiteSpace(trackingId)) return false;
            return _byTracking.TryGet(trackingId, out issue);
        }
        public IEnumerable<IssueReport> AllSortedByTrackingId()
            => _byTracking.InOrder().Select(pair => pair.Value);

        ///------------------------------------
        /// Returns top issues
        ///------------------------------------
        public IEnumerable<IssueReport> TopUrgent(int k)
        {
            if (k <= 0) yield break;

            var snap = new MinHeap<IssueReport>((x, y) => IssueUrgency.Compare(x, y));
            foreach (var (_, v) in _byTracking.InOrder())
            {
                if (v is null) continue;
                snap.Push(v);
            }

            for (int i = 0; i < k && snap.Count > 0; i++)
                yield return snap.Pop();
        }
    }
}
