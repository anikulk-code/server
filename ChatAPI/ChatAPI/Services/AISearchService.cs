﻿using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Services
{
    public class AISearchService
    {
        public const string url = "https://aisearchani.search.windows.net";

        private readonly ILogger<AISearchService> logger;
        SearchIndexClient indexClient;
        public AISearchService(ILogger<AISearchService> logger)
        {
            this.logger = logger;
        }

        public async Task<string> Search(string query)
        {
            string key = Environment.GetEnvironmentVariable("AI_SEARCH_API_KEY");
            const string indexName = "docs-index1";
            SearchClient searchClient = new SearchClient(new Uri(url), indexName, new AzureKeyCredential(key));

            var searchResults = await searchClient.SearchAsync<SearchDocument>(query);
            if (searchResults==null)
            {
                logger.LogError("No results found for query: " + query);
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
