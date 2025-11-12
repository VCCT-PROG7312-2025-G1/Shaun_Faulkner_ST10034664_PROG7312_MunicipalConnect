using Microsoft.AspNetCore.Mvc;
using MunicipalConnect.Infrastructure;
using MunicipalConnect.Models;
using MunicipalConnect.ViewModels;
using MunicipalConnect.Data;

namespace MunicipalConnect.Controllers
{
    [Route("[controller]")]
    public class EventsAndAnnouncementsController : Controller
    {

        ///------------------------------------
        /// <summary>
        /// Handles listing and searching of events and announcements
        /// </summary>
        ///------------------------------------

        private readonly IEventService _serv;
        public EventsAndAnnouncementsController(IEventService serv) => _serv = serv;

        ///------------------------------------
        /// Gets events and announcements
        ///------------------------------------

        [HttpGet("Event")]
        public IActionResult Event([FromQuery] EventFilter filter)
        {
            var csv = Request.Query["CategoriesCSV"].ToString();
            if (!string.IsNullOrWhiteSpace(csv))
            {
                foreach (var item in csv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    filter.Categories.Add(item);
                }
            }

            var results = _serv.Search(filter);

            var vm = new EventViewModel
            {
                Filter = filter,
                Groups = _serv.GroupedByDate(results).ToList(),
                Announcements = _serv.GetActiveAnnouncement(DateTime.UtcNow).ToList(),
                AllCategories = _serv.AllCategories,
                AllLocations = _serv.AllLocations,
                AllTypes = Enum.GetValues(typeof(EventType)).Cast<EventType>(),
                Recommended = _serv.RecommendedForFilter(filter, 6)
            };

            return View("EventsAndAnnouncements", vm);
            

        }

        ///------------------------------------
        /// Shows partial with event details and similar events
        ///------------------------------------

        [HttpGet("Details/{id:guid}")]
        public IActionResult EventDetails(Guid id)
        {
            var eve = _serv.Search(new EventFilter { IsUpcoming = false }).FirstOrDefault(eve => eve.Id == id);
            if (eve is null)
                return NotFound();

            var vm = new EventDetailsViewModal
            {
                Event = eve,
                Similar = _serv.GetSimilarEvents(eve, 6)
            };

            return PartialView("EventDetailsView", vm);
        }
    }
}
