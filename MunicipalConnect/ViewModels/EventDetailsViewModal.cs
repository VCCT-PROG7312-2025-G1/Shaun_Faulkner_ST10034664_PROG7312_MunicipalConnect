using MunicipalConnect.Models;

namespace MunicipalConnect.ViewModels
{
    public class EventDetailsViewModal
    {
        public Event Event { get; set; } = default!;
        public IEnumerable<Event> Similar {  get; set; } = Enumerable.Empty<Event>();
    }
}
