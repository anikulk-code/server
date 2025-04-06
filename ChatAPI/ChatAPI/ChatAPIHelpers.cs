using Azure;
using Azure.AI.Inference;

namespace ChatAPI
{
    internal static class ChatAPIHelpers
    {

        public static async Task<string> callAzureService(string chatMessage)
        {
            var endpoint = new Uri(Environment.GetEnvironmentVariable("API_URL"));
            var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("API_KEY"));
            var model = "gpt-4o-mini";

            var client = new ChatCompletionsClient(
                endpoint,
                credential);

            var requestOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestUserMessage(chatMessage)
                },
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