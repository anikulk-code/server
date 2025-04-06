using Azure;
using Azure.AI.Inference;

namespace ChatAPI
{
    internal static class ChatAPIHelpers
    {

        public static async Task<string> callAzureService(string[] chatMessages)
        {
            var endpoint = new Uri(Environment.GetEnvironmentVariable("MODEL_API_URL"));
            var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("MODEL_API_KEY"));
            var model = "gpt-4o-mini";

            var client = new ChatCompletionsClient(
                endpoint,
                credential);

            var requestOptions = new ChatCompletionsOptions()
            {
                Messages = chatMessages.Select(m => new ChatRequestUserMessage(m)).ToArray(),
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