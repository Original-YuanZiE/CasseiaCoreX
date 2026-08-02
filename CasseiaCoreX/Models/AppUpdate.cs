using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CasseiaCoreX.Models
{
    public class AppUpdate
    {
        public class GitHubReleaseInfo
        {
            public string TagName { get; set; }
            public string HtmlUrl { get; set; }
            public List<Asset> Assets { get; set; }
        }

        public class Asset
        {
            public string Name { get; set; }
            public string BrowserDownloadUrl { get; set; }
            public long Size { get; set; }
        }

        public async Task<GitHubReleaseInfo> FetchLatestReleaseAsync(string owner, string repo, string token = null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36 Edg/152.0.0.0");
            client.Timeout = TimeSpan.FromSeconds(15);

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                
                throw new Exception($"GitHub API Error: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return new GitHubReleaseInfo
            {
                TagName = doc.RootElement.GetProperty("tag_name").GetString(),
                HtmlUrl = doc.RootElement.GetProperty("html_url").GetString(),
                Assets = doc.RootElement.GetProperty("assets").EnumerateArray()
                    .Select(a => new Asset
                    {
                        Name = a.GetProperty("name").GetString(),
                        BrowserDownloadUrl = a.GetProperty("browser_download_url").GetString(),
                        Size = a.GetProperty("size").GetInt64()
                    }).ToList()
            };
        }

        public bool IsNewerVersion(string currentVersion, string latestTag)
        {
            if (latestTag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                latestTag = latestTag.Substring(1);

            // 补全为 4 段版本号（例如 "1.0.0" → "1.0.0.0"）
            string NormalizeVersion(string version)
            {
                var parts = version.Split('.');
                if (parts.Length >= 4) return version;
                // 补 .0 直到 4 段
                return version + string.Concat(Enumerable.Repeat(".0", 4 - parts.Length));
            }

            string curNormalized = NormalizeVersion(currentVersion);
            string latNormalized = NormalizeVersion(latestTag);

            if (!Version.TryParse(curNormalized, out var cur) ||
                !Version.TryParse(latNormalized, out var lat))
                return false;

            return lat > cur;
        }
    }
}
