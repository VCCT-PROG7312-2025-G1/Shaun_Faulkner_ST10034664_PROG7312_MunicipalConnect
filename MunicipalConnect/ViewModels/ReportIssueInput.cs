using System.ComponentModel.DataAnnotations;
using MunicipalConnect.Models;

namespace MunicipalConnect.ViewModels
{
    public class ReportIssueInput
    {
        [Required(ErrorMessage = "Please provide a location (address or landmark).")]
        [Display(Name = "Location (address or landmark)")]
        [StringLength(140, ErrorMessage = "Location is too long (max {1} characters).")]
        public string Location { get; set; } = "";

        [Required(ErrorMessage = "Please select a category.")]
        public IssueCategory? Category { get; set; }

        [Required(ErrorMessage = "Please describe the issue.")]
        [MinLength(20, ErrorMessage = "Description is too short (min {1} characters).")]
        [StringLength(1000, ErrorMessage = "Description is too long (max {1} characters).")]
        public string Description { get; set; } = "";
    }
}