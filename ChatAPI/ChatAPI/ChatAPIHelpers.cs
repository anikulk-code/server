using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.Logging;

namespace ChatAPI
{
    internal static class ChatAPIHelpers
    {

        public static async Task<string> callAzureService(string[] chatMessages, ILogger<ChatAPI> _logger)
        {
            var endpoint = new Uri(Environment.GetEnvironmentVariable("MODEL_API_URL"));
            var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("MODEL_API_KEY"));

            if (string.IsNullOrEmpty(endpoint?.ToString()) || string.IsNullOrEmpty(credential?.ToString()))
            {
                return "Could not read endpoint or credential";
            }

            var model = "gpt-4o-mini";

            var client = new ChatCompletionsClient(
                endpoint,
                credential);

            if (chatMessages==null||chatMessages.Count() ==0)
            {
                _logger.LogError("Chat messages are null or empty");
                return "Chat messages are null or empty";
            }
            else
            {
                _logger.LogInformation("Chat messages count before calling AI API:" + chatMessages.Count());
            }

            List<ChatRequestMessage> chatMessagesList = new List<ChatRequestMessage>();
            foreach (var message in chatMessages)
            {
                chatMessagesList.Add(new ChatRequestUserMessage(message));
            }

            var requestOptions = new ChatCompletionsOptions()
            {
                Messages = chatMessagesList,
                //Messages =
                //{
                //    chatMessages.Select(m => new ChatRequestUserMessage(m)).ToArray(),
                //    new ChatRequestUserMessage(chatMessages.LastOrDefault())
                //},
                MaxTokens = 4096,
                Temperature = 1.0f,
                //top_p = 1.0f,
                Model = model
            };

            Response<ChatCompletions> response = await client.CompleteAsync(requestOptions);
            return response.Value.Content;
        }
    }
}