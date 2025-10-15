using Microsoft.AspNetCore.Components;
using MunicipalConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalConnect.Infrastructure
{
    public interface IEventService
    {
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

            foreach (var cate in eve.Categories)
            {
                _categories.Add(cate);
            }

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
                eve.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                eve.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
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
    }
}
