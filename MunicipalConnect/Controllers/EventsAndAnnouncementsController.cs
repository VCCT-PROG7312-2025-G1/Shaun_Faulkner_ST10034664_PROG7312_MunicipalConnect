using Microsoft.AspNetCore.Mvc;
using MunicipalConnect.Infrastructure;
using MunicipalConnect.Models;
using MunicipalConnect.ViewModels;

namespace MunicipalConnect.Controllers
{
    public class EventsAndAnnouncementsController : Controller
    {

        private readonly IEventService _serv;

        public EventsAndAnnouncementsController(IEventService serv) => _serv = serv;

        [HttpGet("Event")]
        public IActionResult Event([FromQuery] EventFilter filter)
        {
            var results = _serv.Search(filter);
            var vm = new EventViewModel
            {
                Filter = filter,
                Groups = _serv.GroupedByDate(results).ToList(),
                Announcements = _serv.GetActiveAnnouncement(DateTime.UtcNow).ToList(),
                AllCategories = _serv.AllCategories,
                AllLocations = _serv.AllLocations
            };

            return View("EventsAndAnnouncements", vm);
            

        }

        public IActionResult Seed()
        {
            var eve1 = new Event
            {
                Title = "Title of event",
                Description = "description",
                Type = EventType.Meetup,
                Location = "Cape Town",
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Categories = new(StringComparer.OrdinalIgnoreCase) { "Category types", "another type" }
            };
            var eve2 = new Event
            {
                Title = "Title of event",
                Description = "description",
                Type = EventType.Meetup,
                Location = "Online",
                StartTime = DateTime.UtcNow.AddDays(3),
                Categories = new(StringComparer.OrdinalIgnoreCase) { "Category types", "another type" }
            };
            _serv.AddEvent(eve1);
            _serv.AddEvent(eve2);

            _serv.AddAnnouncement(new Announcement
            {
                Title = "Welcome",
                Description = "description",
                StartFrom = DateTime.UtcNow,
                Sticky = true,
                Priority = 10,
            });

            return RedirectToAction(nameof(Event));
        }
    }
}
