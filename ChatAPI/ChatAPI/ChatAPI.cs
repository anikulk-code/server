using ChatAPI.Misc;
using ChatAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Text.Json;


namespace ChatAPI
{
    public class ChatAPI
    {
        private readonly ILogger<ChatAPI> _logger;
        private readonly AISearchService aiSearch;
        private readonly CompletionService completionService;

        public ChatAPI(ILogger<ChatAPI> logger, AISearchService aiSearch, CompletionService completionService  )
        {
            _logger = logger;
            this.aiSearch=aiSearch;
            this.completionService=completionService;
        }

        [Function("ChatAPI")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            ChatRequest chatMessages = null;

            try
            {
                string requestBody = string.Empty;

                (chatMessages, requestBody) = await GetChatMessagesFromRequest(req, _logger);
                if (string.IsNullOrEmpty(requestBody) || chatMessages == null)
                {
                    var errorResponse = HandleErrorCase();
                    return new OkObjectResult(errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body");
                var errorResponse = HandleErrorCase();
                return new OkObjectResult(errorResponse);
            }
            var response = req.HttpContext.Response;
            response.Headers.Append("Access-Control-Allow-Origin", "*");

            string context = await ExtractUserContext(chatMessages);


            string urlContext = await FetchUrlContent(chatMessages);

            var responseFromModel = await completionService.callAzureService(chatMessages, context??"", urlContext??"", _logger);
            var responseData = new
            {
                reply = responseFromModel,
                date = DateTime.UtcNow
            };
            return new OkObjectResult(responseData);
        }

        private async Task<string> FetchUrlContent(ChatRequest chatMessages)
        {
            string urlContext = string.Empty;
            string userQuery = ExtractUserQuery(chatMessages);
            if (!string.IsNullOrEmpty(userQuery))
            {
                var urls = Utils.ExtractUrl(userQuery, _logger);
                if (urls!=null && urls.Count() > 0)
                {
                    foreach (var url in urls)
                    {
                        string crawledContent = await Utils.CrawlUrl(url, _logger);
                        if (!string.IsNullOrEmpty(crawledContent))
                        {
                            urlContext += crawledContent;
                        }
                    }
                }
            }

            return urlContext;
        }

        private async Task<string> ExtractUserContext(ChatRequest chatMessages)
        {
            string context = string.Empty;
            string userQuery = ExtractUserQuery(chatMessages);
            if (string.IsNullOrEmpty(userQuery))
            {
                context = await aiSearch.Search(userQuery);
            }
            if (string.IsNullOrEmpty(context))
            {
                context = await aiSearch.Search("*");
            }

            return context;
        }

        private static string ExtractUserQuery(ChatRequest chatMessages)
        {
            return chatMessages.Messages.Where(m => m.Role=="user").LastOrDefault()?.Content;
        }

        private static async Task<(ChatRequest, string requestBody)> GetChatMessagesFromRequest(HttpRequest req, ILogger<ChatAPI> logger)
        {
            ChatRequest chatRequest = null;
            using (StreamReader reader = new StreamReader(req.Body))
            {
                string requestBody = await reader.ReadToEndAsync();

                try
                {
                    if (string.IsNullOrEmpty(requestBody))
                    {
                        logger.LogError("Request body is null");
                        return (chatRequest, requestBody);
                    }
                    logger.LogInformation("Request body: " + requestBody);
                    chatRequest = JsonSerializer.Deserialize<ChatRequest>(requestBody);
                    return (chatRequest, requestBody);

                }
                catch (JsonException ex)
                {
                    // Handle JSON parsing error
                    Console.WriteLine($"JSON parsing error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle other errors
                    Console.WriteLine($"Error: {ex.Message}");
                }
                return (chatRequest, requestBody);
            }
        }

        private static Response HandleErrorCase()
        {
            var message = "The chat message needs to be in a prop called message";
            Response responseData = new Response()
            {
                reply = message,
                date = DateTime.UtcNow
            };
            return responseData;
        }
    }
}
