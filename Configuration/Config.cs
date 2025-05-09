using Newtonsoft.Json;
using SophBot.Messages;

namespace SophBot.Configuration {
    public class Config {
        private static string configFile = $"{AppDomain.CurrentDomain.BaseDirectory}/config.json";

        public static class Discord {
            public static string Token = "errorToken";
        }
        public static class PSQL {
            public static string Host = "errorSQL";
            public static string Database = "errorSQL";
            public static string Username = "errorSQL";
            public static string Password = "errorSQL";
        }
        public static class Twitch {
            public static string ClientId = "errorTwitch";
            public static string ClientSecret = "errorTwitch";
            public static string AccessToken = "errorTwitch";
        }
        public static class AI
        {
            public static string Key = "errorAI";
        }
        

        public static async ValueTask ReadAsnyc() {
            try {
                using (StreamReader sr = new StreamReader(configFile)) {
                    var file = await sr.ReadToEndAsync();

                    if (file == null) throw new Exception($"Coulnd't read config file from {configFile}");

                    var data = JsonConvert.DeserializeObject<configStructure>(file);

                    if (data == null) throw new Exception("Coulnd't convert config data");

                    Discord.Token = data.token;
                    PSQL.Host = data.psql_host;
                    PSQL.Database = data.psql_database;
                    PSQL.Username = data.psql_username;
                    PSQL.Password = data.psql_password;
                    Twitch.ClientId = data.twitch_clientid;
                    Twitch.ClientSecret = data.twitch_clientsecret;
                    Twitch.AccessToken = await getTwitchAccessTokenAsync();
                    AI.Key = data.ai_api_key;
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }

        private static async ValueTask<string> getTwitchAccessTokenAsync()
        {
            string destination = "https://id.twitch.tv/oauth2/token";
            HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                {"client_id", Twitch.ClientId},
                {"client_secret", Twitch.ClientSecret },
                {"grant_type", "client_credentials" },
                {"scope", "chat:read chat:edit user:bot" }
            };
            var request = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(destination, request);
            var responseString = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<tokenResponse>(responseString);
            if (data != null)
            {
                return data.access_token;
            }
            else
            {
                return "Access Token request failed";
                throw new Exception("Access Token request failed");
            }
        }

        internal class configStructure {
            public string token = "errorToken";
            public string psql_host = "errorSQL";
            public string psql_username = "errorSQL";
            public string psql_database = "errorSQL";
            public string psql_password = "errorSQL";
            public string twitch_clientid = "errorTwitch";
            public string twitch_clientsecret = "errorTwitch";
            public string ai_api_key = "errorAI";
        }
        internal class tokenResponse 
        {
            public string access_token = "errorTwitch";
        }
    
    }
}