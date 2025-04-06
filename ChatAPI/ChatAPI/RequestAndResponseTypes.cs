using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatAPI
{
    public class Response
    {
        public string? reply { get; set; }
        public DateTime date { get; set; }
    }


    public class ChatRequest
    {
        [JsonPropertyName("message")]
        public List<ChatMessage> Messages { get; set; }
    }

    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

}
