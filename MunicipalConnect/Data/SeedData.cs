using MunicipalConnect.Infrastructure;
using MunicipalConnect.Models;

namespace MunicipalConnect.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Used ChatGPT to generate Seed data based on my modals and eventypes.
        /// https://chatgpt.com/
        /// </summary>
        /// <param name="eventService"></param>
        public static void Initialize(IEventService eventService)
        {
            if (eventService.Search(new EventFilter { IsUpcoming = false }).Any())
                return;

            var events = new List<Event>
            {
                new Event
                {
                    Title = "Ward 45 Community Meeting",
                    Description = "Join councillor Maree at the Blaauwberg Hall to discuss local infrastructure projects and safety initiatives.",
                    Type = EventType.CommunityMeeting,
                    Location = "Blaauwberg Community Hall, Table View",
                    StartTime = DateTime.UtcNow.AddDays(2).AddHours(18),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Community", "Safety", "Infrastructure" }
                },
                new Event
                {
                    Title = "Public Hearing: Water Tariff Adjustments",
                    Description = "The City invites residents to voice opinions on the proposed 2025 water tariff structure.",
                    Type = EventType.PublicHearing,
                    Location = "Cape Town Civic Centre",
                    StartTime = DateTime.UtcNow.AddDays(4).AddHours(10),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Water", "Finance", "Public" }
                },
                new Event
                {
                    Title = "Recycling Workshop for Households",
                    Description = "Learn best practices for waste separation and recycling from the City’s Environmental Affairs team.",
                    Type = EventType.Workshop,
                    Location = "Milnerton Library Hall",
                    StartTime = DateTime.UtcNow.AddDays(5).AddHours(9),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Environment", "Waste", "Education" }
                },
                new Event
                {
                    Title = "Road Maintenance and Upgrades Briefing",
                    Description = "An overview of scheduled roadworks in the Blaauwberg and Parklands areas.",
                    Type = EventType.Infrastructure,
                    Location = "Blaauwberg Subcouncil Office",
                    StartTime = DateTime.UtcNow.AddDays(6).AddHours(11),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Roads", "Traffic", "Planning" }
                },
                new Event
                {
                    Title = "Emergency Response Simulation Drill",
                    Description = "Joint disaster management exercise focusing on coastal flooding preparedness.",
                    Type = EventType.EmergencyResponse,
                    Location = "Table View Beachfront",
                    StartTime = DateTime.UtcNow.AddDays(8).AddHours(8),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Disaster Management", "Safety" }
                },
                new Event
                {
                    Title = "Community Clean-up Drive: Lagoon Beach",
                    Description = "Residents and volunteers are invited to participate in cleaning Lagoon Beach. Bags and gloves provided.",
                    Type = EventType.CleanUpDrive,
                    Location = "Lagoon Beach, Milnerton",
                    StartTime = DateTime.UtcNow.AddDays(10).AddHours(9),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Environment", "Community" }
                },
                new Event
                {
                    Title = "Online Workshop: Water-Saving Techniques",
                    Description = "Virtual session teaching residents how to reduce water consumption at home.",
                    Type = EventType.Online,
                    Location = "Online",
                    StartTime = DateTime.UtcNow.AddDays(3).AddHours(17),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Water", "Sustainability" }
                },
                new Event
                {
                    Title = "Scheduled Water Supply Outage – Parklands North",
                    Description = "Maintenance on the main supply line will cause a temporary outage. Tankers will be deployed where needed.",
                    Type = EventType.ServiceOutage,
                    Location = "Parklands North",
                    StartTime = DateTime.UtcNow.AddDays(1).AddHours(6),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Water", "Maintenance" }
                },
                new Event
                {
                    Title = "Electricity Infrastructure Upgrade",
                    Description = "The City will replace aging transformers to improve reliability in Table View.",
                    Type = EventType.Infrastructure,
                    Location = "Table View Substation",
                    StartTime = DateTime.UtcNow.AddDays(7).AddHours(8),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Electricity", "Maintenance" }
                },
                new Event
                {
                    Title = "Community Meeting: Park Safety",
                    Description = "Discuss safety measures and lighting upgrades for public parks in the ward.",
                    Type = EventType.CommunityMeeting,
                    Location = "Sunningdale Sports Complex",
                    StartTime = DateTime.UtcNow.AddDays(9).AddHours(18),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Community", "Safety", "Parks" }
                },
                new Event
                {
                    Title = "Public Hearing: Noise Regulation Updates",
                    Description = "Provide your feedback on proposed updates to municipal noise control by-laws.",
                    Type = EventType.PublicHearing,
                    Location = "Cape Town City Hall",
                    StartTime = DateTime.UtcNow.AddDays(11).AddHours(10),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Law", "Public", "Noise" }
                },
                new Event
                {
                    Title = "Online Townhall: Digital Service Access",
                    Description = "An interactive online session on using e-Services for rates, billing, and reporting.",
                    Type = EventType.Online,
                    Location = "Online",
                    StartTime = DateTime.UtcNow.AddDays(12).AddHours(17),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Technology", "Community" }
                },
                new Event
                {
                    Title = "Health and Wellness Workshop",
                    Description = "Free blood pressure checks, fitness guidance, and wellness talks.",
                    Type = EventType.Workshop,
                    Location = "Table View Community Clinic",
                    StartTime = DateTime.UtcNow.AddDays(13).AddHours(9),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Health", "Community" }
                },
                new Event
                {
                    Title = "Unplanned Power Outage – Milnerton Ridge",
                    Description = "Technicians are working to restore supply following a cable fault. Expected resolution in 6 hours.",
                    Type = EventType.ServiceOutage,
                    Location = "Milnerton Ridge",
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    Categories = new(StringComparer.OrdinalIgnoreCase) { "Electricity", "Emergency" }
                }
            };

            foreach (var e in events)
                eventService.AddEvent(e);

            var announcements = new List<Announcement>
            {
                new Announcement
                {
                    Title = "Welcome to MunicipalConnect",
                    Description = "Stay informed about Cape Town’s municipal events, maintenance, and community programs.",
                    StartFrom = DateTime.UtcNow,
                    Sticky = true,
                    Priority = 10
                },
                new Announcement
                {
                    Title = "Waste Collection Reminder",
                    Description = "Waste collection days have changed for Blaauwberg and Parklands areas effective next week.",
                    StartFrom = DateTime.UtcNow.AddDays(-1),
                    Sticky = false,
                    Priority = 8
                },
                new Announcement
                {
                    Title = "Severe Weather Alert",
                    Description = "Heavy rainfall expected over the weekend. Avoid low-lying areas and check stormwater drains near your property.",
                    StartFrom = DateTime.UtcNow,
                    Sticky = false,
                    Priority = 9
                },
                new Announcement
                {
                    Title = "Online Payment System Maintenance",
                    Description = "The City’s online payment portal will be unavailable Sunday from 1 AM to 4 AM for scheduled maintenance.",
                    StartFrom = DateTime.UtcNow.AddDays(1),
                    Sticky = false,
                    Priority = 5
                },
                new Announcement
                {
                    Title = "Community Development Grants Open",
                    Description = "Applications are now open for NGOs and youth initiatives through the 2025 Community Development Fund.",
                    StartFrom = DateTime.UtcNow.AddDays(3),
                    Sticky = false,
                    Priority = 7
                }
            };

            foreach (var ann in announcements)
                eventService.AddAnnouncement(ann);
        }
    }
}
