using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SophBot.bot.conf;
using SophBot.bot.logs;

namespace SophBot.bot.ai
{
    public class SGroqEngine
    {
        public static async ValueTask<string> GenerateResponseAsync(string promt)
        {
            string url = "https://api.groq.com/openai/v1/chat/completions";
            var model = "compound-beta";

            promt = promt.Replace('"', ' ');

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SConfig.AI.GroqKey);

                var json = $@"{{
                    ""messages"": [
                        {{""role"": ""system"",""content"": ""{SConfig.AI.SystemInstructions}""}},
                        {{ ""role"": ""user"", ""content"": ""{promt}"" }}],
                    ""model"": ""{model}""
                    }}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();


                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(result))
                    {
                        var root = doc.RootElement;

                        var reply = root
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        if (reply == null) return "AI-Anfrage fehlgeschlagen - Bitte kontaktiere den Entwickler dieses Bots! (reply is null)";

                        return reply;
                    }
                }
                catch 
                {
                    throw;
                }
            }
        }
    }
}