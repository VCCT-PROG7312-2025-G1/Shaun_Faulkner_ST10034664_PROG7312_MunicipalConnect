using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MunicipalConnect.Models;

namespace MunicipalConnect.Infrastructure
{
    public static class TrackingIdGenerator
    {
        private static readonly Dictionary<IssueCategory, string> Prefix = new()
        {
            { IssueCategory.Roads,        "RD" },
            { IssueCategory.Water,        "WT" },
            { IssueCategory.Electricity,  "EL" },
            { IssueCategory.Sanitation,   "SN" },
            { IssueCategory.Waste,        "WS" },
            { IssueCategory.Parks,        "PK" },
            { IssueCategory.PublicSafety, "PS" },
            { IssueCategory.Other,        "OT" },
        };

        public static string Generate(IssueCategory category, IEnumerable<IssueReport> existing)
        {
            var cat = Prefix.TryGetValue(category, out var p) ? p : "OT";
            var today = DateTimeOffset.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"MC-{cat}-{today}-";

            var max = existing
                .Where(r => r.TrackingId != null && r.TrackingId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(r =>
                {
                    var tail = r.TrackingId.Split('-')[^1];
                    return int.TryParse(tail, out var n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(max + 1):D4}";
        }
    }
}
