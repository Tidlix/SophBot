using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Web;

namespace TwitchSharp
{
    public static class TwitchSharpEngine
    {
        #region Variables
        private static ConsoleLevel _ConsoleLevel = ConsoleLevel.Information;
        private static bool _ShowTime = false;
        private static bool _ShowConsoleLevel = false;
        private static string _DateTimeFormat = "dd/MM/yyyy - HH:mm:ss";
        #endregion

        #region Enums
        public enum ConsoleLevel
        {
            Trace,
            Debug,
            Information,
            Warning,
            Error,
            Needed
        }
        #endregion

        #region Methods
        public static void ModifyEngine(ConsoleLevel? consoleLevel = null, bool? showTime = null, bool? showConsoleLevel = null, string? dateTimeFormat = null)
        {
            if (consoleLevel != null) _ConsoleLevel = (ConsoleLevel)consoleLevel;
            if (showTime != null) _ShowTime = (bool)showTime;
            if (showConsoleLevel != null) _ShowConsoleLevel = (bool)showConsoleLevel;
            if (dateTimeFormat != null) _DateTimeFormat = (string)dateTimeFormat;
        }
        public static void SendConsole(string content, ConsoleLevel level)
        {
            if (level < _ConsoleLevel) return;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"[TwitchSharp{(_ShowTime ? " " + DateTime.Now.ToString(_DateTimeFormat) : "")}] {(_ShowConsoleLevel ? $" {level} - " : "")}");
            Console.ResetColor();

            Console.WriteLine(content);
        }
        public static async Task<string> GenerateRefreshTokenAsync(TwitchRefreshTokenConfig conf)
        {
            for (int i = 0; i < conf.Scopes.Length; i++) conf.Scopes[i] = conf.Scopes[i].Replace(":", "%3A");
            string link = $"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={conf.ClientID}&redirect_uri={conf.RedirectUri}&scope={string.Join('+', conf.Scopes)}";

            SendConsole("Authorization needed! Please authorize with the following link: \n" + link, ConsoleLevel.Needed);

            Console.Write("\nAfter redirect, insert new url here > ");
            string? responseUrl = Console.ReadLine();
            if (responseUrl == null) throw new Exception("Authorization code can't be null!");
            Console.Clear();
            var uri = new Uri(responseUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            string? auth = queryParams["code"];

            if (string.IsNullOrEmpty(auth))
                throw new Exception("Authorization code was not found in response url!");

            using (var httpClient = new HttpClient())
            {
                Dictionary<string, string> parameters = new()
                {
                    {"client_id", $"{conf.ClientID}"},
                    {"client_secret", $"{conf.ClientSecret}"},
                    {"code", auth},
                    {"grant_type", "authorization_code"},
                    {"redirect_uri", conf.RedirectUri}
                };

                string destination = "https://id.twitch.tv/oauth2/token";
                var content = new FormUrlEncodedContent(parameters);

                var response = await httpClient.PostAsync(destination, content);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        JsonElement json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                        return json.GetProperty("refresh_token").GetString()!;
                    default:
                        throw new Exception($"An Error occured while trying to generate User Access Token - Status code: {response.StatusCode}");
                }
            }
        }
        #endregion
    }
    public class TwitchRefreshTokenConfig
    {
        public required string ClientID { get; set; }
        public required string ClientSecret { get; set; }
        public required string RedirectUri { get; set; }
        public required string[] Scopes { get; set; }
    }
}