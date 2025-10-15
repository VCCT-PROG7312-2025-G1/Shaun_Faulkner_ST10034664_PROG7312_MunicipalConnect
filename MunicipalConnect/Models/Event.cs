using System;
using System.Collections.Generic;

namespace MunicipalConnect.Models
{

    public enum EventType { Meetup, Onsite, Randomitem }
    public class Event
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public EventType Type { get; set; }
        public string Location { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public HashSet<string> Categories { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public bool IsActive { get; set; }

    }

    public class EventFilter
    {
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public EventType? Type { get; set; }
        public string? Location { get; set; }
        public HashSet<string> Categories { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string? Query { get; set; }
        public bool IsUpcoming { get; set; } = true;
    }
}
