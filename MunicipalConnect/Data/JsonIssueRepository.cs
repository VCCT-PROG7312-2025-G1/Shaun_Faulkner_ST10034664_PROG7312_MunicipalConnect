using System.Text.Json;
using MunicipalConnect.Models;

namespace MunicipalConnect.Data
{
    ///------------------------------------
    /// <summary>
    /// Storage for IssueReport
    /// </summary>
    ///------------------------------------
    public interface IIssueRepository
    {
        Task AddAsync(IssueReport issue, CancellationToken ct = default);
        Task<IssueReport?> GetByTrackingIdAsync(string trackingId, CancellationToken ct = default);
        Task<IReadOnlyList<IssueReport>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(IssueReport issue, CancellationToken ct = default);
    }

    ///------------------------------------
    /// Stores into JSON file locally
    ///------------------------------------

    public sealed class JsonIssueRepository : IIssueRepository
    {
        private readonly string _path;
        private static readonly SemaphoreSlim _gate = new(1, 1);

        public JsonIssueRepository(IConfiguration cfg)
        {
            _path = cfg["Data:IssuesPath"] ?? Path.Combine("wwwroot", "data", "issues.json");
        }

        private sealed class Box
        {
            public List<IssueReport> Items { get; set; } = new();
        }

        private async Task<Box> LoadAsync()
        {
            if (!File.Exists(_path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                return new Box();
            }

            await using var fs = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                var data = await JsonSerializer.DeserializeAsync<Box>(fs);
                return data ?? new Box();
            }
            catch (JsonException)
            {
                return new Box();
            }
        }

        ///------------------------------------
        /// Writes to whole file
        ///------------------------------------
        private async Task SaveAsync(Box box)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            var tmp = _path + ".tmp";
            await using (var fs = File.Create(tmp))
            {
                await JsonSerializer.SerializeAsync(fs, box,
                    new JsonSerializerOptions { WriteIndented = true });
            }

            if (File.Exists(_path))
                File.Replace(tmp, _path, null);
            else
                File.Move(tmp, _path);
        }

        ///------------------------------------
        /// Writes to file to keep consistent
        ///------------------------------------

        public async Task AddAsync(IssueReport issue, CancellationToken ct = default)
        {
            await _gate.WaitAsync(ct);
            try
            {
                var box = await LoadAsync();
                box.Items.Add(issue);
                await SaveAsync(box);
                Console.WriteLine($"[JsonRepo] Added issue: {issue.TrackingId}");
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<IssueReport?> GetByTrackingIdAsync(string trackingId, CancellationToken ct = default)
        {
            var box = await LoadAsync();
            return box.Items.FirstOrDefault(i =>
                string.Equals(i.TrackingId, trackingId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IReadOnlyList<IssueReport>> GetAllAsync(CancellationToken ct = default)
        {
            var box = await LoadAsync();
            return box.Items;
        }

        public async Task UpdateAsync(IssueReport issue, CancellationToken ct = default)
        {
            await _gate.WaitAsync(ct);
            try
            {
                var box = await LoadAsync();
                var ix = box.Items.FindIndex(i => i.TrackingId == issue.TrackingId);
                if (ix >= 0)
                    box.Items[ix] = issue;
                else
                    box.Items.Add(issue);

                await SaveAsync(box);
                Console.WriteLine($"[JsonRepo] Updated issue: {issue.TrackingId}");
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
