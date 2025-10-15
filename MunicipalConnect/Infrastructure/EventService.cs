using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Components;
using MunicipalConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalConnect.Infrastructure
{
    public interface IEventService
    {
        /// <summary>
        /// https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-9.0
        /// </summary>
        /// <param name="e"></param>
        
        void AddEvent(Event e);
        void AddAnnouncement(Announcement a);
        IEnumerable<Event> Search(EventFilter filter);
        IEnumerable<(DateOnly Date, IEnumerable<Event> Items)> GroupedByDate(IEnumerable<Event> items);
        IEnumerable<Event> GetUpcoming(int count);
        IEnumerable<Announcement> GetActiveAnnouncement(DateTime nowUtc);
        IReadOnlyCollection<string> AllCategories {  get; }
        IReadOnlyCollection<string> AllLocations { get; }
        void MarkViewed(Guid eventId);
        IEnumerable<Guid> RecentlyViewed(int max = 10);

        IEnumerable<Event> GetSimilarEvents(Event target, int max = 6);
        IEnumerable<Event> RecommendedForFilter(EventFilter filter, int max = 6);

    }

    public class EventService : IEventService
    {
        private readonly Dictionary<Guid, Event> _byId = new();
        private readonly SortedDictionary<DateOnly, List<Event>> _byDate = new();
        private readonly HashSet<string> _categories = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _locations = new(StringComparer.OrdinalIgnoreCase);
        private readonly PriorityQueue<Event, DateTime> _upcoming = new();
        private readonly PriorityQueue<Announcement, int> _announcementPrio = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));
        private readonly Queue<Announcement> _announcementQueue = new();
        private readonly Stack<Guid> _recentlyViewed = new();

        public IReadOnlyCollection<string> AllCategories => _categories;
        public IReadOnlyCollection<string> AllLocations => _locations;

        public void AddEvent(Event eve)
        {
            _byId[eve.Id] = eve;

            var date = DateOnly.FromDateTime(eve.StartTime);
            if (!_byDate.TryGetValue(date, out var list))
            {
                list = new List<Event>();
                _byDate[date] = list;
            }
            list.Add(eve);

            if (eve.Categories != null)
                foreach (var cate in eve.Categories)
                    _categories.Add(cate);

            if (!string.IsNullOrEmpty(eve.Location))
                _locations.Add(eve.Location);

            if (eve.StartTime >= DateTime.UtcNow)
                _upcoming.Enqueue(eve, eve.StartTime);
        }

        public void AddAnnouncement(Announcement ann)
        {
            _announcementQueue.Enqueue(ann);
            _announcementPrio.Enqueue(ann, (ann.Sticky ? 1_000_000 : 0) + ann.Priority);
        }

        public IEnumerable<Event> GetUpcoming(int count)
        {
            var clone = _upcoming.UnorderedItems
                                 .Select(x => x.Element)
                                 .Where(eve => eve.StartTime >= DateTime.UtcNow)
                                 .OrderBy(eve => eve.StartTime)
                                 .Take(count);

            return clone;
        }

        public IEnumerable<Announcement> GetActiveAnnouncement(DateTime nowUtc)
        {
            var high = _announcementPrio.UnorderedItems
                .Select (x => x.Element)
                .Where(ann => ann.StartFrom <= nowUtc && (ann.EndAt is null || nowUtc <= ann.EndAt))
                .Distinct()
                .ToList();

            var first = _announcementQueue.Where(ann => ann.StartFrom <= nowUtc && (ann.EndAt is null || nowUtc <= ann.EndAt));

            return high.Concat(first).Distinct();
        }

        public IEnumerable<Event> Search(EventFilter fil)
        {
            IEnumerable<Event> source;

            if (fil.IsUpcoming)
            {
                source = _upcoming.UnorderedItems
                    .Select(x => x.Element)
                    .Where(eve => eve.StartTime >= DateTime.UtcNow);
            }
            else
            {
                source = _byId.Values;
            }

            if (fil.FromTime is not null)
            {
                source = source
                    .Where(eve => eve.StartTime >= fil.FromTime.Value);
            }

            if (fil.ToTime is not null)
            {
                source = source
                    .Where(eve => eve.StartTime <= fil.ToTime.Value);
            }

            if (fil.Type is not null)
            {
                source = source
                    .Where(eve => eve.Type == fil.Type.Value);
            }

            if (!string.IsNullOrWhiteSpace(fil.Location))
            {
                source = source
                    .Where(eve => string.Equals(eve.Location, fil.Location, StringComparison.OrdinalIgnoreCase));
            }

            if (fil.Categories.Count > 0)
            {
                source = source
                    .Where(eve => eve.Categories.Overlaps(fil.Categories));
            }

            if (!string.IsNullOrWhiteSpace(fil.Query))
            {
                var query = fil.Query.Trim();
                source = source.Where(eve =>
                    (eve.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (eve.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            return source.OrderBy(eve => eve.StartTime);
        }

        public IEnumerable<(DateOnly Date, IEnumerable<Event> Items)> GroupedByDate(IEnumerable<Event> items)
        {
            return items.GroupBy(eve => DateOnly.FromDateTime(eve.StartTime))
                .OrderBy(group => group.Key)
                .Select(group => (group.Key, group.AsEnumerable()));
        }

        public void MarkViewed(Guid eventId)
        {
            if (_byId.ContainsKey(eventId))
                _recentlyViewed.Push(eventId);
        }

        public IEnumerable<Guid> RecentlyViewed(int max = 10)
            => _recentlyViewed.Take(max);

        public IEnumerable<Event> GetSimilarEvents(Event target, int max = 6)
        {
            var pool = _byId.Values.Where(eve => eve.Id != target.Id);

            var list = new List<(Event eve, double score)>();
            foreach (var eve in pool)
            {
                int overlap = (target.Categories == null || eve.Categories == null)
                    ? 0
                    : target.Categories.Intersect(eve.Categories, StringComparer.OrdinalIgnoreCase).Count();

                if (overlap == 0) continue;

                double score = 0;
                score += overlap * 3;
                if (eve.Type == target.Type) score += 2;

                if (!string.IsNullOrWhiteSpace(target.Location) &&
                    !string.IsNullOrWhiteSpace(eve.Location) &&
                    string.Equals(eve.Location, target.Location, StringComparison.OrdinalIgnoreCase))
                    score += 1;

                var daysGap = Math.Abs((eve.StartTime - target.StartTime).TotalDays);
                score -= daysGap / 45.0;

                list.Add((eve, score));
            }

            return list.OrderByDescending(x => x.score)
                       .ThenBy(x => x.eve.StartTime)
                       .Take(max)
                       .Select(x => x.eve);
        }

        public IEnumerable<Event> RecommendedForFilter(EventFilter filter, int max = 6)
        {
            var source = _byId.Values.AsEnumerable();

            var targetCats = (filter.Categories != null)
                ? new HashSet<string>(filter.Categories, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var list = new List<(Event eve, double score)>();
            foreach (var ev in source)
            {
                double score = 0;

                if (targetCats.Any() && ev.Categories?.Any() == true)
                {
                    int overlap = targetCats.Intersect(ev.Categories, StringComparer.OrdinalIgnoreCase).Count();
                    if (overlap == 0) continue;
                    score += overlap * 3;
                }

                if (filter.Type is not null && ev.Type == filter.Type.Value) score += 2;

                if (!string.IsNullOrWhiteSpace(filter.Location) &&
                    !string.IsNullOrWhiteSpace(ev.Location) &&
                    string.Equals(ev.Location, filter.Location, StringComparison.OrdinalIgnoreCase))
                    score += 1;

                if (score <= 0) continue;
                list.Add((ev, score));
            }

            return list.OrderByDescending(x => x.score)
                       .ThenBy(x => x.eve.StartTime)
                       .Take(max)
                       .Select(x => x.eve);
        }
    }
}
