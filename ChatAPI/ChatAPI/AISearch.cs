using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;

namespace ChatAPI
{
    public class AISearch
    {
        public const string url = "https://aisearchani.search.windows.net";

        private readonly ILogger<AISearch> logger;
        SearchIndexClient indexClient;
        public AISearch(ILogger<AISearch> logger)
        {
            this.logger = logger;
        }

        public async Task<string> Search(string query)
        {
            string key = Environment.GetEnvironmentVariable("AI_SEARCH_API_KEY");
            const string indexName = "docs-index1";
            SearchClient searchClient = new SearchClient(new Uri(url), indexName, new AzureKeyCredential(key));

            var userQuery = "Tell me more about Aniruddha?";
            var searchResults = await searchClient.SearchAsync<SearchDocument>(userQuery);
            if (searchResults==null)
            {
                logger.LogError("No results found for query: " + userQuery);
                return string.Empty;
            }

            var topDocs = new List<string>();
            await foreach (SearchResult<SearchDocument> result in searchResults.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue("content", out var content))
                {
                    topDocs.Add(content.ToString());
                }
            }

            string context = string.Join("\n---\n", topDocs.Take(3)); // Limit to top 3 snippets

            return context;
        }
    }
}
