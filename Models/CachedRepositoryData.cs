using System.Security.Cryptography;
using System.Text;

namespace Arma_3_LTRM.Models
{
    public class CachedRepositoryData
    {
        public string RepositoryId { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public DateTime LastScanned { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int CacheVersion { get; set; } = 1;
        public string RepositorySnapshot { get; set; } = string.Empty;
        public Dictionary<string, List<CachedFtpItem>> DirectoryCache { get; set; } = new();
        public int TotalFiles { get; set; }
        public int TotalDirectories { get; set; }
        public long TotalSizeBytes { get; set; }

        public bool IsExpired() => DateTime.Now >= ExpiresAt;

        public bool IsValidForRepository(Repository repo, TimeSpan cacheLifetime)
        {
            if (IsExpired())
                return false;

            var currentSnapshot = GenerateRepositorySnapshot(repo);
            if (RepositorySnapshot != currentSnapshot)
                return false;

            return true;
        }

        public static string GenerateRepositorySnapshot(Repository repo)
        {
            var data = $"{repo.Url}:{repo.Port}:{repo.Username}";
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
