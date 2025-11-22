using SophBot.Twitch.EventHandlers;
using TwitchSharp;
using TwitchSharp.Entities;
using TwitchSharp.Events.Types;

namespace SophBot.Twitch
{
    public static class TwitchEngine
    {
#pragma warning disable CS8618 
        public static TwitchClient Client;
#pragma warning restore CS8618

        public static async Task Initialize(string clientId, string clientSecret)
        {
            string token = await TwitchSharpEngine.GenerateRefreshTokenAsync(new ()
            {
                ClientID = clientId,
                ClientSecret = clientSecret,
                RedirectUri = "https://localhost:3000",
                Scopes = ["user:bot", "user:read:chat", "user:write:chat", "user:read:whispers", "user:manage:whispers", "moderator:read:chatters", "moderator:read:followers", "moderator:read:moderators", ]
            });
            var clientConf = new TwitchClientConfig()
            {
                ClientID = clientId,
                ClientSecret = clientSecret,
                RefreshToken = token
            };
            Client = new TwitchClient(clientConf);

            var events = Client.UseEvents();
            events.OnChannelChatMessageReceived += async (s, e) => await ChannelChatMessageReceivedHandler.OnReceive(s, e); 
            events.OnClientWhisperReceived += async (s, e) => await ClientWhisperReceivedHandler.OnReceive(s, e);

            TwitchUser mainBroadcaster = await Client.GetUserByLoginAsync("tidlix");

            await events.SubscribeToEventAsync(new ChannelChatMessageReceivedEvent(mainBroadcaster));
            await events.SubscribeToEventAsync(new ClientWhisperReceivedEvent());
        }
    }
}