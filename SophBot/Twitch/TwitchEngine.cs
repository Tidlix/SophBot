using System.Text.Json;
using SophBot.Universal;
using TwitchSharp.Api;
using TwitchSharp.Api.Authentication;
using TwitchSharp.Api.Clients;

namespace SophBot.Twitch
{
    public static class TwitchEngine
    {
        #pragma warning disable CS8618
        public static TwitchApiClient MainClient { get; private set; } // Main client with bot token
        public static UserData BotUserData { get; private set; }
        public static TwitchApiClient SubClient { get; private set; } // Sub client with broadcaster token
        #pragma warning restore CS8618

        public static async Task Initialize()
        {
            MainClient = TwitchApiClient.Create(new TwitchApiClientOptions()
            {
                ClientId = Config.Twitch.ClientId,
                ClientSecret = Config.Twitch.ClientSecret
            });
            MainClient.SetUserToken(await generateUserTokenSet(Config.Twitch.MainRFToken));
            
            SubClient = TwitchApiClient.Create(new TwitchApiClientOptions()
            {
                ClientId = Config.Twitch.ClientId,
                ClientSecret = Config.Twitch.ClientSecret
            });
            SubClient.SetUserToken(await generateUserTokenSet(Config.Twitch.SubRFToken));

            BotUserData = (await MainClient.Users.GetUsersAsync(logins: ["sophbotv3"]))[0];
        }

        public static async Task SendMessageAsync(UserData channel, string message) 
        {
            await MainClient.Chat.SendChatMessageAsync(new SendMessageRequest()
            {
                BroadcasterId = channel.Id,
                SenderId = BotUserData.Id,
                Message = message
            });
        }
        public static async Task SendWhisperAsync(UserData user, string message)
        {
            await MainClient.Whispers.SendWhisperAsync(
                BotUserData.Id,
                user.Id,
                new SendWhisperRequest()
                {
                    Message = message
                });
        }

        private static async Task<TwitchTokenSet> generateUserTokenSet (string refreshToken)
        {
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "client_id", Config.Twitch.ClientId },
                    { "client_secret", Config.Twitch.ClientSecret },
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken }
                };

                var content = new FormUrlEncodedContent(parameters);

                try
                {
                    var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to refresh token: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

                    var scopes = new List<string>();
                    if (tokenResponse.TryGetProperty("scope", out var scopeProperty))
                    {
                        if (scopeProperty.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var scope in scopeProperty.EnumerateArray())
                            {
                                if (scope.GetString() is string scopeValue)
                                {
                                    scopes.Add(scopeValue);
                                }
                            }
                        }
                        else if (scopeProperty.GetString() is string scopeString)
                        {
                            scopes.AddRange(scopeString.Split(' '));
                        }
                    }

                    return new TwitchTokenSet()
                    {
                        AccessToken = tokenResponse.GetProperty("access_token").GetString() ?? "",
                        RefreshToken = tokenResponse.GetProperty("refresh_token").GetString() ?? "",
                        ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.GetProperty("expires_in").GetInt32()),
                        Scopes = scopes,
                        TokenType = tokenResponse.GetProperty("token_type").GetString() ?? ""
                    };
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception($"HTTP request error while refreshing token: {ex.Message}", ex);
                }
                catch (JsonException ex)
                {
                    throw new Exception($"Failed to parse token response: {ex.Message}", ex);
                }
            }
        }
    }
}