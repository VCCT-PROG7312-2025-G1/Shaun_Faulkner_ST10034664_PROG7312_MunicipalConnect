using System;
using System.Collections.Generic;
using MunicipalConnect.Models;

namespace MunicipalConnect.Infrastructure.Indexing
{
    public static class IssueUrgency
    {
        ///------------------------------------
        /// Maps status with lower being most important
        ///------------------------------------
        public static int Weight(IssueStatus s) => s switch
        {
            IssueStatus.Submitted => 0,
            IssueStatus.InProgress => 1,
            IssueStatus.Resolved => 2,
            IssueStatus.Closed => 3,
            _ => 4
        };

        ///------------------------------------
        /// Total ordering for issuereport
        ///------------------------------------
        public static int Compare(IssueReport? a, IssueReport? b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a is null) return 1;
            if (b is null) return -1;

            int byStatus = Weight(a.Status).CompareTo(Weight(b.Status));
            if (byStatus != 0) return byStatus;

            int byCreated = a.CreatedAt.CompareTo(b.CreatedAt);
            if (byCreated != 0) return byCreated;

            int byCategory = a.Category.CompareTo(b.Category);
            if (byCategory != 0) return byCategory;

            return string.Compare(a.TrackingId, b.TrackingId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOpen(IssueStatus s) =>
            s == IssueStatus.Submitted || s == IssueStatus.InProgress;

        public static IComparer<IssueReport> Comparer { get; } =
            System.Collections.Generic.Comparer<IssueReport>.Create(Compare);
    }
}
