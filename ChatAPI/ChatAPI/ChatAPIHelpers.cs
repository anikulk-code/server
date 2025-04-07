using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.Logging;

namespace ChatAPI
{
    internal static class ChatAPIHelpers
    {

        public static async Task<string> callAzureService(ChatRequest chatRequest, string userContext, string contextFromUrl, ILogger<ChatAPI> _logger)
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

            if (!string.IsNullOrEmpty(userContext))
            {
                chatMessagesList.Add(new ChatRequestAssistantMessage("Please use this context about the user: " + userContext));
            }

            if (!string.IsNullOrEmpty(contextFromUrl))
            {
                chatMessagesList.Add(new ChatRequestAssistantMessage("Please use this context from the url mentioned in the query: " + contextFromUrl));
            }

            int userMessageCount = 0;
            int assistantMessageCount = 0;
            foreach (var message in chatRequest.Messages)
            {
                if (string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))
                {
                    chatMessagesList.Add(new ChatRequestUserMessage(message.Content));
                    userMessageCount++;
                }
                else if (string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                {
                    chatMessagesList.Add(new ChatRequestAssistantMessage(message.Content));
                    assistantMessageCount++;
                }
                else
                {
                    _logger.LogError("Invalid role in chat message: " + message.Role);
                }
                if(userMessageCount>=3 && assistantMessageCount>= 3)
                {
                    break;
                }
            }

            var requestOptions = new ChatCompletionsOptions()
            {
                Messages = chatMessagesList,
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
