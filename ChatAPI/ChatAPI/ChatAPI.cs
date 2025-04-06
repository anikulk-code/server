using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Text.Json;


namespace ChatAPI
{
    public class ChatAPI
    {
        private readonly ILogger<ChatAPI> _logger;

        public ChatAPI(ILogger<ChatAPI> logger)
        {
            _logger = logger;
        }

        [Function("ChatAPI")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string[] chatMessages = null;

            try
            {
                string requestBody = string.Empty;

                (chatMessages, requestBody) = await GetChatMessagesFromRequest(req);
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

            var responseFromModel = await ChatAPIHelpers.callAzureService(chatMessages, _logger);
            var responseData = new
            {
                reply = responseFromModel,
                date = DateTime.UtcNow
            };
            return new OkObjectResult(responseData);
        }

        private static async Task<(string[] chatMessages, string requestBody)> GetChatMessagesFromRequest(HttpRequest req)
        {
            using (StreamReader reader = new StreamReader(req.Body))
            {
                string requestBody = await reader.ReadToEndAsync();

                try
                {
                    JsonDocument json = JsonDocument.Parse(requestBody);
                    var messages = json.RootElement.GetProperty("message");
                    if (messages.ValueKind == JsonValueKind.Array && messages.GetArrayLength() > 0)
                    {
                        var tempChatMessages = messages.EnumerateArray().Select(m => m.GetString()).Where(m => !string.IsNullOrEmpty(m)).ToArray();
                        if (tempChatMessages.Length>0)
                        {
                            return (tempChatMessages, requestBody);
                        }
                    }
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
                return (Array.Empty<string>(), requestBody);
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
class Response
{
    public string? reply { get; set; }
    public DateTime date { get; set; }
}
