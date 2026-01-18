using System.IO;
using System.Text.Json;
using Arma_3_LTRM.Models;

namespace Arma_3_LTRM.Services
{
    public class FtpCacheManager
    {
        private const string CACHE_FOLDER = "Settings/Cache";
        private readonly TimeSpan _defaultCacheLifetime = TimeSpan.FromHours(1);

        public FtpCacheManager()
        {
            if (!Directory.Exists(CACHE_FOLDER))
            {
                Directory.CreateDirectory(CACHE_FOLDER);
            }
        }

        private string GetCacheFilePath(string repositoryId)
        {
            var safeId = string.Join("_", repositoryId.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(CACHE_FOLDER, $"{safeId}_cache.json");
        }

        public CachedRepositoryData? LoadCache(string repositoryId)
        {
            try
            {
                var filePath = GetCacheFilePath(repositoryId);

                if (!File.Exists(filePath))
                    return null;

                var json = File.ReadAllText(filePath);
                var cache = JsonSerializer.Deserialize<CachedRepositoryData>(json);

                return cache;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cache: {ex.Message}");
                return null;
            }
        }

        public void SaveCache(CachedRepositoryData cache)
        {
            try
            {
                var filePath = GetCacheFilePath(cache.RepositoryId);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(cache, options);
                File.WriteAllText(filePath, json);

                System.Diagnostics.Debug.WriteLine($"Cache saved: {cache.TotalFiles} files, {cache.TotalDirectories} directories");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving cache: {ex.Message}");
            }
        }

        public void InvalidateCache(string repositoryId)
        {
            try
            {
                var filePath = GetCacheFilePath(repositoryId);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"Cache invalidated for repository: {repositoryId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error invalidating cache: {ex.Message}");
            }
        }

        public string GetCacheInfo(string repositoryId)
        {
            var cache = LoadCache(repositoryId);

            if (cache == null)
                return "No cache available";

            var age = DateTime.Now - cache.LastScanned;
            var status = cache.IsExpired() ? "Expired" : "Valid";

            return $"Last scanned: {FormatAge(age)} ago ({status})";
        }

        private string FormatAge(TimeSpan age)
        {
            if (age.TotalMinutes < 1)
                return "just now";
            if (age.TotalHours < 1)
                return $"{(int)age.TotalMinutes} minutes";
            if (age.TotalDays < 1)
                return $"{(int)age.TotalHours} hours";
            return $"{(int)age.TotalDays} days";
        }

        public void ClearExpiredCaches()
        {
            try
            {
                if (!Directory.Exists(CACHE_FOLDER))
                    return;

                var files = Directory.GetFiles(CACHE_FOLDER, "*_cache.json");

                foreach (var file in files)
                {
                    var json = File.ReadAllText(file);
                    var cache = JsonSerializer.Deserialize<CachedRepositoryData>(json);

                    if (cache?.IsExpired() == true)
                    {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"Deleted expired cache: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing expired caches: {ex.Message}");
            }
        }
    }
}
