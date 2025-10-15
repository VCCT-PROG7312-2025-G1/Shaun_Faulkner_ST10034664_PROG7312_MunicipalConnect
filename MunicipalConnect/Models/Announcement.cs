namespace MunicipalConnect.Models
{
    public class Announcement
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EndAt { get; set; }
        public int Priority { get; set; } = 0;
        public bool Sticky { get; set; } = false;
    }
}
