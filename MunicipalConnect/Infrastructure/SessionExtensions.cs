using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MunicipalConnect.Models;

namespace MunicipalConnect.Infrastructure
{
    public static class SessionExtensions
    {
        private const string IssueMapKey = "IssueMapSorted";

        public static SortedDictionary<string, IssueReport> GetIssuesMap(this ISession session)
        {
            var json = session.GetString(IssueMapKey);
            return string.IsNullOrEmpty(json)
                ? new SortedDictionary<string, IssueReport>()
                : (JsonSerializer.Deserialize<SortedDictionary<string, IssueReport>>(json)
                   ?? new SortedDictionary<string, IssueReport>());
        }

        public static void SaveIssuesMap(this ISession session, SortedDictionary<string, IssueReport> map)
        {
            var json = JsonSerializer.Serialize(map);
            session.SetString(IssueMapKey, json);
        }
    }
}
