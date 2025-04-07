using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ChatAPI
{
    public static class Utils
    {
        const string pattern = @"https?:\/\/[^\s]+|www\.[^\s]+";

        public static string[] ExtractUrl(string queryText, ILogger logger)
        {
            var matches = Regex.Matches(queryText, pattern);
            List<string> urls = new List<string>();
            foreach (Match match in matches)
            {
                logger.LogInformation($"Detected URL: {match.Value}");
                urls.Add(match.Value);
            }
            return urls.ToArray();
        }


        public async static Task<string> CrawlUrl(string url, ILogger logger)
        {
            try
            {
                HttpClient client = new HttpClient();
                string html = await client.GetStringAsync(url);

                // 2. Clean HTML
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                foreach (var node in doc.DocumentNode.SelectNodes("//script|//style") ?? Enumerable.Empty<HtmlNode>())
                    node.Remove();
                string content = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
                string plainText = string.Join("\n", content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)));
                return plainText;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error crawling URL: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
