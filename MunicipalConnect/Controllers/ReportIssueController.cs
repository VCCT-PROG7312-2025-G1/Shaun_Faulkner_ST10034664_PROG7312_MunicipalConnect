using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MunicipalConnect.Infrastructure;
using MunicipalConnect.Models;
using MunicipalConnect.ViewModels;

namespace MunicipalConnect.Controllers
{
    public class ReportIssuesController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ReportIssuesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult Create() => View(new ReportIssueInput());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReportIssueInput input)
        {
            if (!ModelState.IsValid) return View(input);

            var map = HttpContext.Session.GetIssuesMap();

            var report = new IssueReport
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                Location = input.Location.Trim(),
                Category = input.Category!.Value,
                Description = input.Description.Trim(),
                Status = IssueStatus.Submitted
            };

            report.TrackingId = TrackingIdGenerator.Generate(report.Category, map.Values);

            var files = Request.Form.Files;
            if (files.Count > 0)
            {
                var baseDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", report.TrackingId);
                Directory.CreateDirectory(baseDir);

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file is null || file.Length == 0) continue;

                    var name = Path.GetFileName(file.FileName);
                    var dest = Path.Combine(baseDir, name);
                    using var stream = System.IO.File.Create(dest);
                    file.CopyTo(stream);

                    report.AttachmentPaths.Add($"/uploads/{report.TrackingId}/{name}");
                }
            }

            map[report.TrackingId] = report;
            HttpContext.Session.SaveIssuesMap(map);

            return RedirectToAction(nameof(Success), new { id = report.TrackingId });
        }

        [HttpGet]
        public IActionResult Success(string id)
        {
            var map = HttpContext.Session.GetIssuesMap();
            if (!map.TryGetValue(id, out var report))
                return RedirectToAction("Index", "Home");

            return View(report);
        }
    }
}
