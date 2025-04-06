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

        public ChatAPI(ILogger<ChatAPI> logger)
        {
            _logger = logger;
        }

        [Function("ChatAPI")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string? chatMessage = null;

            try
            {
                string requestBody = string.Empty;

                using (StreamReader reader = new StreamReader(req.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                    JsonDocument json = JsonDocument.Parse(requestBody);
                    chatMessage = json.RootElement.GetProperty("message").GetString();
                }
                if (string.IsNullOrEmpty(requestBody))
                {
                    chatMessage = "The chat message needs to be in a prop called message";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body");
                //return new BadRequestObjectResult("Error reading request body");
            }
            var response = req.HttpContext.Response;
            response.Headers.Append("Access-Control-Allow-Origin", "*");

            var responseFromModel = await ChatAPIHelpers.callAzureService(chatMessage);
            var responseData = new
            {
                reply = responseFromModel,
                date = DateTime.UtcNow
            };
            return new OkObjectResult(responseData);
        }
    }
}
