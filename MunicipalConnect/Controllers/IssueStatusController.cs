using Microsoft.AspNetCore.Mvc;
using MunicipalConnect.Data;
using MunicipalConnect.Infrastructure.Indexing;
using MunicipalConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalConnect.Controllers
{
    ///------------------------------------
    /// <summary>
    /// Handles issue browsing, filters and tracking
    /// </summary>
    ///------------------------------------
    public sealed class IssueStatusController : Controller
    {
        
        private readonly IIssueIndex _index;
        private readonly IIssueRepository _repo;

        public IssueStatusController(IIssueIndex index, IIssueRepository repo)
        {
            _index = index;
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Index(
            string sort = "urgent",
            IssueStatus? status = null,
            IssueCategory? category = null,
            string? q = null,
            int page = 1,
            int pageSize = 50)
        {
            ///------------------------------------
            /// Defaults for paging 
            ///------------------------------------
            
            page = Math.Max(1, page);
            pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 500);

            ///------------------------------------
            /// Start with urgent issues or all sorted by ID
            ///------------------------------------

            IEnumerable<IssueReport> baseSeq =
                string.Equals(sort, "id", StringComparison.OrdinalIgnoreCase)
                    ? _index.AllSortedByTrackingId()
                    : _index.TopUrgent(page * pageSize);

            ///------------------------------------
            /// Filters
            ///------------------------------------

            if (status is not null)
                baseSeq = baseSeq.Where(r => r.Status == status.Value);

            if (category is not null)
                baseSeq = baseSeq.Where(r => r.Category == category.Value);

            ///------------------------------------
            /// Search query
            ///------------------------------------

            if (!string.IsNullOrWhiteSpace(q))
            {
                var needle = q.Trim();
                baseSeq = baseSeq.Where(r =>
                    (!string.IsNullOrEmpty(r.Location) && r.Location.Contains(needle, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.Contains(needle, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.TrackingId) && r.TrackingId.Contains(needle, StringComparison.OrdinalIgnoreCase))
                );
            }

            ///------------------------------------
            /// Pagination
            ///------------------------------------

            var total = baseSeq.Count();
            var items = baseSeq.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Sort = sort;
            ViewBag.Query = q;
            ViewBag.Status = status;
            ViewBag.Category = category;

            return View(items);
        }

        [HttpGet]
        public IActionResult Track(string trackingId)
        {

            ///------------------------------------
            /// Input check
            ///------------------------------------

            if (string.IsNullOrWhiteSpace(trackingId))
                return BadRequest("Missing trackingId.");

            if (_index.TryGetByTrackingId(trackingId, out var issue))
                return View(issue);

            return NotFound($"Tracking ID '{trackingId}' not found.");
        }
    }
}