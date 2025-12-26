using Microsoft.Extensions.Logging;
using SophBot.Twitch.Events;
using SophBot.Universal;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Models;
using TwitchSharp;

namespace SophBot.Twitch
{
    public static class TwitchEngine {
        #pragma warning disable CS8618
        public static TwitchSharp.TwitchClient TwitchSharpClient;
        public static TwitchLib.Client.TwitchClient TwitchLibClient;
        public static LiveStreamMonitorService MonitorService;
        #pragma warning restore CS8618

        public static async Task Initialize()
        {
            TwitchRefreshTokenConfig tokenConf = new ()
            {
                ClientID = Config.Twitch.ClientId,
                ClientSecret = Config.Twitch.ClientSecret,
                RedirectUri = "https://localhost:3000",
                Scopes = [
                    "chat:read",
                    "chat:edit",
                    "user:bot",
                    "user:read:chat",
                    "user:write:chat",
                    "user:read:whispers",
                    "user:manage:whispers",
                    "moderator:read:chatters",
                    "moderator:read:followers",
                    "moderator:read:moderators"
                ]
            };
            string refreshToken = await TwitchSharpEngine.GenerateRefreshTokenAsync(tokenConf);

            TwitchClientConfig clientConf = new ()
            {
                ClientID = Config.Twitch.ClientId,
                ClientSecret = Config.Twitch.ClientSecret,
                RefreshToken = refreshToken
            };
            TwitchSharpClient = new TwitchSharp.TwitchClient(clientConf);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .ClearProviders()
                    .AddProvider(new Logs.LogProvider())
                    .SetMinimumLevel(LogLevel.Debug);
            });

            TwitchLibClient = new TwitchLib.Client.TwitchClient(loggerFactory: loggerFactory);
            ConnectionCredentials credentials = new ConnectionCredentials(TwitchSharpClient.CurrentUser.LoginName, await TwitchSharpClient.GetUserAccessTokenAsync());
            TwitchLibClient.Initialize(credentials, TwitchSharpClient.CurrentUser.LoginName);

            TwitchAPI api = new();
            api.Settings.ClientId = Config.Twitch.ClientId;
            api.Settings.Secret = Config.Twitch.ClientSecret;
            api.Settings.AccessToken = await TwitchSharpClient.GetAppAccessTokenAsync();

            MonitorService = new LiveStreamMonitorService(api, 10);
            MonitorService.OnStreamOnline += async (s, e) => await StreamOnlineEvents.OnStreamOnlineAsync(s, e);


            TwitchLibClient.ChatCommandIdentifiers.Add("!");
            //TwitchLibClient.ChatCommandIdentifiers.Add($"@{TwitchSharpClient.CurrentUser.LoginName}"); 
            //TwitchLibClient.ChatCommandIdentifiers.Add($"@{TwitchSharpClient.CurrentUser.DisplayName}"); 
            TwitchLibClient.OnChatCommandReceived += Commands.CommandHandler.OnCommandSend;

            TwitchLibClient.OnMessageReceived += MessageEvents.OnMessageReceived;
            
            

            await TwitchLibClient.ConnectAsync();

            await TwitchLibClient.JoinChannelAsync("xsophe");
            await TwitchLibClient.JoinChannelAsync("tidlix");

        }
    }
}