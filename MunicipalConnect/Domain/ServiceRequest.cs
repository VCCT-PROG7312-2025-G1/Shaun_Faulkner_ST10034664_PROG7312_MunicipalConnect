
namespace MunicipalConnect.Domain
{
    public enum RequestStatus { Submitted, InProgress, OnHold, Resolved, Closed, Reopened }

    public sealed class StatusEntry
    {
        public DateTime At { get; set; }
        public RequestStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public sealed class ServiceRequest
    {
        public string TrackingId { get; set; }
        public string ReporterName { get; set; }
        public string Category { get; set; } = default!;
        public string Location { get; set; } = default!;
        public int Priority { get; set; }
        public DateTime CreateAt { get; set; }
        public RequestStatus CurrentStatus { get; set; }
        public List<StatusEntry> History { get; } = new();
    }
}
