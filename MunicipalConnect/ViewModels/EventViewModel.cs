using MunicipalConnect.Models;

namespace MunicipalConnect.ViewModels
{
    public class EventViewModel
    {
        public EventFilter Filter { get; set; } = new();
        public List<(DateOnly Date, IEnumerable<Event> Items)> Groups { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
        public IReadOnlyCollection<string> AllCategories { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> AllLocations { get; set; } = Array.Empty<string>();
        public IEnumerable<EventType> AllTypes { get; set; } = (EventType[])Enum.GetValues(typeof(EventType));
    }
}
