using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MunicipalConnect.Data;
using MunicipalConnect.Infrastructure;
using MunicipalConnect.Infrastructure.Indexing;
using MunicipalConnect.Models;
using MunicipalConnect.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MunicipalConnect.Controllers
{
    public class ReportIssuesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IIssueRepository _repo;
        private readonly IIssueIndex _index;

        ///------------------------------------
        /// Upload types
        ///------------------------------------

        private static readonly HashSet<string> AllowedExt =
            new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".pdf", ".docx", ".webp", ".txt" };

        ///------------------------------------
        /// File upload size
        ///------------------------------------

        private const long MaxUploadBytes = 10 * 1024 * 1024;

        public ReportIssuesController(
            IWebHostEnvironment env,
            IIssueRepository repo,
            IIssueIndex index)
        {
            _env = env;
            _repo = repo;
            _index = index;
        }

        [HttpGet]
        public IActionResult Create() => View(new ReportIssueInput());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportIssueInput input)
        {
            if (!ModelState.IsValid) return View(input);

            var report = new IssueReport
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                Location = input.Location.Trim(),
                Category = input.Category!.Value,
                Description = input.Description.Trim(),
                Status = IssueStatus.Submitted
            };

            ///------------------------------------
            /// Generate tracking ID
            ///------------------------------------

            var existing = await _repo.GetAllAsync();
            report.TrackingId = TrackingIdGenerator.GenerateFromIds(
                report.Category,
                existing.Select(i => i.TrackingId)
            );

            ///------------------------------------
            /// File upload
            ///------------------------------------

            var files = Request.Form?.Files;
            if (files != null && files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var f = files[i];
                    if (f is null || f.Length == 0) continue;

                    if (f.Length > MaxUploadBytes)
                    {
                        ModelState.AddModelError("", $"File '{f.FileName}' exceeds {MaxUploadBytes / (1024 * 1024)} MB.");
                        return View(input);
                    }

                    var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                    if (!AllowedExt.Contains(ext))
                    {
                        ModelState.AddModelError("", $"File type not allowed: {ext}");
                        return View(input);
                    }
                }

                ///------------------------------------
                /// Uploads create path
                ///------------------------------------

                var webroot = _env.WebRootPath ?? "wwwroot";
                var baseDir = Path.Combine(webroot, "uploads", report.TrackingId);
                Directory.CreateDirectory(baseDir);

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file is null || file.Length == 0) continue;

                    var safeName = SanitizeFileName(Path.GetFileName(file.FileName));
                    var uniqueName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{safeName}";
                    var dest = Path.Combine(baseDir, uniqueName);

                    try
                    {
                        await using var stream = System.IO.File.Create(dest);
                        await file.CopyToAsync(stream);

                        report.AttachmentPaths.Add($"/uploads/{report.TrackingId}/{uniqueName}");
                    }
                    catch (IOException)
                    {
                        ModelState.AddModelError("", $"Could not save file '{file.FileName}'. Please try again.");
                        return View(input);
                    }
                }
            }

            await _repo.AddAsync(report);
            _index.Upsert(report);

            return RedirectToAction(nameof(Success), new { id = report.TrackingId });
        }

        [HttpGet]
        public async Task<IActionResult> Success(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction("Index", "Home");

            if (_index.TryGetByTrackingId(id, out var fromIndex))
                return View(fromIndex);

            var fromRepo = await _repo.GetByTrackingIdAsync(id);
            if (fromRepo is null) return RedirectToAction("Index", "Home");
            return View(fromRepo);
        }

        ///------------------------------------
        /// Replaces strange characters
        ///------------------------------------
        
        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            name = name.Replace("/", "_").Replace("\\", "_");
            return name.Trim();
        }
    }
}