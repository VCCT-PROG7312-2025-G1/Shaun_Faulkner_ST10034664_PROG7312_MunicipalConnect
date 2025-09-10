using System;
using System.Collections.Generic;

namespace MunicipalConnect.Models
{
    public class IssueReport
    {
        public Guid Id { get; set; }
        public string TrackingId { get; set; } = "";
        public DateTimeOffset CreatedAt { get; set; }

        public string Location { get; set; } = "";
        public IssueCategory Category { get; set; }
        public string Description { get; set; } = "";
        public HashSet<string> AttachmentPaths { get; set; } = new();
        public IssueStatus Status { get; set; }
    }
}
