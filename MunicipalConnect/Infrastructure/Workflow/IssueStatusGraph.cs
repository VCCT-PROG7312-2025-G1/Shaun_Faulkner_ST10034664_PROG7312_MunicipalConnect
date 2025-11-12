using MunicipalConnect.Models;
using System.Collections.Generic;

namespace MunicipalConnect.Infrastructure.Workflow
{

    ///------------------------------------
    /// <summary>
    /// Directed graph
    /// https://www.w3schools.com/dsa/dsa_theory_graphs.php
    /// https://www.simplilearn.com/tutorials/c-sharp-tutorial/what-is-graphs-in-c-sharp
    /// https://www.geeksforgeeks.org/c-sharp/c-sharp-data-structures/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    ///------------------------------------
    public sealed class Graph<T> where T : notnull
    {
        private readonly Dictionary<T, List<T>> _adj = new();

        ///------------------------------------
        /// One directed edge, confirms vertices existence
        ///------------------------------------
        public void AddEdge(T u, T v)
        {
            if (!_adj.ContainsKey(u)) _adj[u] = new List<T>();
            if (!_adj.ContainsKey(v)) _adj[v] = new List<T>();
            _adj[u].Add(v);
        }
        public bool Reachable(T start, T target)
        {
            var q = new Queue<T>(); var seen = new HashSet<T>();
            q.Enqueue(start); seen.Add(start);
            while (q.Count > 0)
            {
                var u = q.Dequeue(); if (EqualityComparer<T>.Default.Equals(u, target)) return true;
                foreach (var v in _adj[u]) if (seen.Add(v)) q.Enqueue(v);
            }
            return false;
        }
    }

    ///------------------------------------
    /// Build
    ///------------------------------------
    public static class IssueStatusGraph
    {
        private static Graph<IssueStatus> Build()
        {
            var g = new Graph<IssueStatus>();
            g.AddEdge(IssueStatus.Submitted, IssueStatus.InProgress);
            g.AddEdge(IssueStatus.InProgress, IssueStatus.Resolved);
            g.AddEdge(IssueStatus.Resolved, IssueStatus.Closed);
            return g;
        }

        public static bool IsAllowedTransition(IssueStatus from, IssueStatus to)
            => Build().Reachable(from, to);
    }
}
