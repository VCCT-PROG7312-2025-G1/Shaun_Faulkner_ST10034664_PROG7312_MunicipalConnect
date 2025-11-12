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
            var existingIds = new HashSet<string>(
                existing.Where(r => !string.IsNullOrWhiteSpace(r?.TrackingId))
                        .Select(r => r.TrackingId!),
                StringComparer.OrdinalIgnoreCase
            );

            return GenerateFromIds(category, existingIds);
        }

        public static string GenerateFromIds(IssueCategory category, IEnumerable<string> existingIds)
        {
            var cat = Prefix.TryGetValue(category, out var p) ? p : "OT";

            var today = DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"MC-{cat}-{today}-";

            var idSet = new HashSet<string>(
                existingIds.Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.OrdinalIgnoreCase
            );

            int maxSeq = 0;
            foreach (var id in idSet)
            {
                if (!id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                var tail = id.Split('-').LastOrDefault();
                if (tail is null) continue;

                if (int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                {
                    if (n > maxSeq) maxSeq = n;
                }
            }

            int next = maxSeq + 1;
            string candidate;
            do
            {
                candidate = $"{prefix}{next:D4}";
                next++;
            } while (idSet.Contains(candidate));

            return candidate;
        }
    }
}
