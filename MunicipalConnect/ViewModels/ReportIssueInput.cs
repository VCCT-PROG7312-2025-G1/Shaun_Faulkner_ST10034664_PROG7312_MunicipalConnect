using System.ComponentModel.DataAnnotations;
using MunicipalConnect.Models;

namespace MunicipalConnect.ViewModels
{
    public class ReportIssueInput
    {
        [Required, Display(Name = "Location (address or landmark)")]
        public string Location { get; set; } = "";

        [Required]
        public IssueCategory? Category { get; set; }

        [Required, MinLength(20)]
        public string Description { get; set; } = "";
    }
}