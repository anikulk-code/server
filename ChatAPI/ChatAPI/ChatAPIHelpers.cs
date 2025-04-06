using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.Logging;

namespace ChatAPI
{
    internal static class ChatAPIHelpers
    {

        public static async Task<string> callAzureService(ChatRequest chatRequest, ILogger<ChatAPI> _logger)
        {
            if (chatRequest==null|| chatRequest.Messages== null || chatRequest.Messages.Count == 0)
            {
                _logger.LogError("Chat messages are null or empty");
                return "Chat messages are null or empty";
            }

            _logger.LogInformation("Chat messages count before calling AI API:" + chatRequest.Messages.Count);

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


            List<ChatRequestMessage> chatMessagesList = new List<ChatRequestMessage>();
            foreach (var message in chatRequest.Messages)
            {
                if (string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))
                {
                    chatMessagesList.Add(new ChatRequestUserMessage(message.Content));
                }
                else if (string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                {
                    chatMessagesList.Add(new ChatRequestAssistantMessage(message.Content));
                }
                else
                {
                    _logger.LogError("Invalid role in chat message: " + message.Role);
                    //chatMessagesList.Add(new ChatRequestSystemMessage(message));
                }
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
