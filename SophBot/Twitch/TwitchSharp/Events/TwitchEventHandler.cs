using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchSharp.Entitys;
using TwitchSharp.Events.Types;
using Websocket.Client;

namespace TwitchSharp.Events
{
    public class TwitchEventHandler
    {
        private TwitchClient _Client { get; set; }
        private string _SessionID { get; set; }
        private List<TwitchEvent> _Events { get; set; }

        private WebsocketClient? _wsClient;

        public event Action<TwitchClient, ClientWhisperReceivedArgs>? OnClientWhisperReceived;
        public event Action<TwitchClient, ChannelChatMessageReceivedArgs>? OnChannelChatMessageReceived;
        public event Action<TwitchClient, ChannelStreamOnlineArgs>? OnChannelStreamOnline;
        public event Action<TwitchClient, ChannelStreamOfflineArgs>? OnChannelStreamOffline;
        public event Action<TwitchClient, ChannelFollowReceicvedArgs>? OnChannelFollowReceived;

        public TwitchEventHandler(TwitchClient client)
        {
            _Client = client;
            _SessionID = "null";
            CreateNewWsClient(new Uri("wss://eventsub.wss.twitch.tv/ws"));
            _Events = new();
        }

        private void CreateNewWsClient(Uri uri)
        {
            var newClient = new WebsocketClient(uri);

            newClient.MessageReceived.Subscribe(msg =>
            {
                if (string.IsNullOrEmpty(msg.Text)) return;

                JsonElement json = JsonDocument.Parse(msg.Text).RootElement;
                var metadata = json.GetProperty("metadata");
                var payload = json.GetProperty("payload");

                string messageType = metadata.GetProperty("message_type").GetString()!;
                switch (messageType)
                {
                    case "session_welcome":
                        var session = payload.GetProperty("session");
                        _SessionID = session.GetProperty("id").GetString()!;
                        TwitchSharpEngine.SendConsole($"WS - Session Welcome with ID {_SessionID}", TwitchSharpEngine.ConsoleLevel.Debug);

                        if (_wsClient is null)
                        {
                            _wsClient = newClient;
                        }
                        else
                        {
                            _wsClient.Dispose();
                            _wsClient = newClient;
                        }
                        break;

                    case "session_keepalive":
                        TwitchSharpEngine.SendConsole("WS - Session Keepalive", TwitchSharpEngine.ConsoleLevel.Trace);
                        break;

                    case "session_reconnect":
                        string reconnectUrl = payload.GetProperty("session").GetProperty("reconnect_url").GetString()!;
                        TwitchSharpEngine.SendConsole("WS - Reconnect requested, waiting for welcome before switching.", TwitchSharpEngine.ConsoleLevel.Debug);
                        CreateNewWsClient(new Uri(reconnectUrl));
                        break;

                    case "revocation":
                        var subscription = payload.GetProperty("subscription");
                        string type = subscription.GetProperty("type").GetString()!;
                        string status = subscription.GetProperty("status").GetString()!;
                        TwitchUser broadcaster = _Client.GetUserByIDAsync(subscription.GetProperty("condition").GetProperty("broadcaster_user_id").GetString()!).Result;
                        TwitchSharpEngine.SendConsole($"Event Subscription {type} for {broadcaster.DisplayName} ({broadcaster.ID}) was revoked and will no longer send events! Status: {status}", TwitchSharpEngine.ConsoleLevel.Warning);
                        break;

                    case "notification":
                        HandleEvent(metadata, payload);
                        break;
                }
            });

            newClient.Start().Wait();
        }

        private void HandleEvent(JsonElement metadata, JsonElement payload)
        {
            string subType = metadata.GetProperty("subscription_type").GetString()!;
            TwitchSharpEngine.SendConsole($"WS - Received Event {subType}", TwitchSharpEngine.ConsoleLevel.Debug);
            switch (subType)
            {
                case "channel.chat.message":
                    OnChannelChatMessageReceived?.Invoke(_Client, new ChannelChatMessageReceivedArgs(_Client, payload));
                    break;
                case "channel.follow":
                    OnChannelFollowReceived?.Invoke(_Client, new ChannelFollowReceicvedArgs(_Client, payload));
                    break;
                case "user.whisper.message":
                    OnClientWhisperReceived?.Invoke(_Client, new ClientWhisperReceivedArgs(_Client, payload));
                    break;
                case "stream.online":
                    OnChannelStreamOnline?.Invoke(_Client, new ChannelStreamOnlineArgs(_Client, payload));
                    break;
                case "stream.offline":
                    OnChannelStreamOffline?.Invoke(_Client, new ChannelStreamOfflineArgs(_Client, payload));
                    break;
            }
        }

        public async Task SubscribeToEventAsync(TwitchEvent twitchEvent)
        {
            using (var httpClient = new HttpClient())
            {
                string url = "https://api.twitch.tv/helix/eventsub/subscriptions";

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _Client.GetUserAccessTokenAsync()}");
                httpClient.DefaultRequestHeaders.Add("Client-Id", _Client.ClientID);

                TwitchUser? user = null;
                bool isBroadcasterEvent = false;
                bool requiresModeratorRole = false;
                if (twitchEvent is IIsBroadcasterEvent bTwitchEvent)
                {
                    isBroadcasterEvent = true;
                    user = bTwitchEvent.Broadcaster;
                    requiresModeratorRole = bTwitchEvent.RequiresModeratorRole;
                }

                var jsonContent = @$"{{ 
                        ""type"": ""{twitchEvent.Type}"", 
                        ""version"": ""{twitchEvent.Version}"", 
                        ""condition"": {{ 
                            {(isBroadcasterEvent ? $"\"broadcaster_user_id\": \"{user!.ID}\"" + (twitchEvent.RequiresUser ? ", " : "") : "")} 
                            {(twitchEvent.RequiresUser ? ((isBroadcasterEvent && requiresModeratorRole) ? "\"moderator_user_id\"" : "\"user_id\"") + $": \"{_Client.CurrentUser.ID}\"" : "")} 
                            }}, 
                        ""transport"": {{ 
                        ""method"": ""websocket"", 
                        ""session_id"": ""{_SessionID}"" 
                        }} 
                    }}";

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(url, content);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    _Events.Add(twitchEvent);
                }
            }
        }
    }

    public class TwitchEvent
    {
        public string Type;
        public int Version;
        public bool RequiresUser;

        public TwitchEvent(string type, int version, bool requiresUser)
        {
            Type = type;
            Version = version;
            RequiresUser = requiresUser;
        }
    }

    public interface IIsBroadcasterEvent
    {
        public TwitchUser Broadcaster { get; }
        public bool RequiresModeratorRole { get; }
    }
}
