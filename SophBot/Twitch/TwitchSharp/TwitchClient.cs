using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.VisualBasic;
using TwitchSharp.Entitys;
using TwitchSharp.Events;

namespace TwitchSharp
{
    public class TwitchClient
    {
        #region Variables
        public string ClientID { get; private set; }
        public TwitchUser CurrentUser { get; private set; }
        private string _ClientSecret { get; set; }
        private string _RefreshToken { get; set; }
        private string _AppAccessToken { get; set; }
        private string _UserAccessToken { get; set; }
        #endregion

        #region Initialization
        public TwitchClient(TwitchClientConfig conf)
        {
            ClientID = conf.ClientID;
            _ClientSecret = conf.ClientSecret;
            _RefreshToken = conf.RefreshToken;
            _AppAccessToken = generateAppTokenAsnyc().Result;
            _UserAccessToken = refreshUserTokenAsnyc().Result;

            ValidateToken(_UserAccessToken, out string currentUserLogin);
            CurrentUser = GetUserByLoginAsync(currentUserLogin).Result;
        }
        #endregion

        #region Tokens
        private bool ValidateToken(string token)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"OAuth {token}");
                var response = httpClient.GetAsync("https://id.twitch.tv/oauth2/validate").Result;

                return response.IsSuccessStatusCode;
            }
        }
        private bool ValidateToken(string token, out string login)
        {
            login = string.Empty;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"OAuth {token}");
                var response = httpClient.GetAsync("https://id.twitch.tv/oauth2/validate").Result;

                if (response.IsSuccessStatusCode)
                {
                    var body = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result).RootElement;
                    login = body.GetProperty("login").GetString()!;
                }
                return response.IsSuccessStatusCode;
            }
        }
        public async Task<string> GetAppAccessTokenAsync()
        {
            if (ValidateToken(_AppAccessToken)) return _AppAccessToken;
            else return await generateAppTokenAsnyc();
        }
        public async Task<string> GetUserAccessTokenAsync()
        {
            if (ValidateToken(_UserAccessToken)) return _UserAccessToken;
            else return await refreshUserTokenAsnyc();
        }
        private async Task<string> generateAppTokenAsnyc()
        {
            using (var httpClient = new HttpClient())
            {
                Dictionary<string, string> parameters = new()
                {
                    {"client_id", $"{ClientID}"},
                    {"client_secret", $"{_ClientSecret}"},
                    {"grant_type", "client_credentials"}
                };

                string destination = "https://id.twitch.tv/oauth2/token";
                var content = new FormUrlEncodedContent(parameters);

                var response = await httpClient.PostAsync(destination, content);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        JsonElement json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                        string token = json.GetProperty("access_token").GetString()!;
                        return token;
                    default:
                        throw new Exception($"An Error occured while trying to generate App Access Token - Status code: {response.StatusCode}");
                }
            }
        }
        private async Task<string> refreshUserTokenAsnyc()
        {
            using (var httpClient = new HttpClient())
            {
                Dictionary<string, string> parameters = new()
                {
                    {"client_id", $"{ClientID}"},
                    {"client_secret", $"{_ClientSecret}"},
                    {"grant_type", $"refresh_token"},
                    {"refresh_token", $"{_RefreshToken}"}
                };

                string destination = "https://id.twitch.tv/oauth2/token";
                var values = new FormUrlEncodedContent(parameters);

                var response = await httpClient.PostAsync(destination, values);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        JsonElement json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                        _UserAccessToken = json.GetProperty("access_token").GetString()!;
                        _RefreshToken = json.GetProperty("refresh_token").GetString()!;
                        return _UserAccessToken;
                    default:
                        throw new Exception($"An Error occured while trying to refresh User Access Token - Status code: {response.StatusCode}");
                }
            }
        }
        #endregion

        #region Users 
        public async Task<TwitchUser> GetUserByIDAsync(string id)
        {
            try
            {
                string url = $"https://api.twitch.tv/helix/users?id={id}";
                JsonElement json = await getUserJsonAsync(url);
                return new TwitchUser(this, json);
            }
            catch
            {
                throw;
            }
        }
        public async Task<TwitchUser> GetUserByLoginAsync(string login)
        {
            try
            {
                string url = $"https://api.twitch.tv/helix/users?login={login}"; // note: login is the name, a user uses to login and is all lower case
                JsonElement json = await getUserJsonAsync(url);
                return new TwitchUser(this, json);
            }
            catch
            {
                throw;
            }
        }
        private async Task<JsonElement> getUserJsonAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await GetAppAccessTokenAsync()}");
                httpClient.DefaultRequestHeaders.Add("Client-Id", $"{ClientID}");

                var response = await httpClient.GetAsync(url);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("data")[0];
                    case HttpStatusCode.BadRequest:
                        throw new Exception("400 - Bad Request! / Invalid user login or id.");
                    case HttpStatusCode.Unauthorized:
                        throw new Exception("401 - Unauthorized! / Invalid token.");
                    default:
                        throw new Exception("Unknown Error!");
                }
            }
        }
        #endregion

        #region Events
        public TwitchEventHandler UseEvents()
        {
            return new TwitchEventHandler(this);
        }
        #endregion
    }

    public class TwitchClientConfig
    {
        public required string ClientID { get; set; }
        public required string ClientSecret { get; set; }
        public required string RefreshToken { get; set; }
    }
}