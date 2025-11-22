using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchSharp.Entities;
using TwitchSharp.Events.Types;
using Websocket.Client;

namespace TwitchSharp.Events
{
    /// <summary>
    /// Handles WebSocket connections and event subscriptions for Twitch EventSub
    /// </summary>
    public class TwitchEventHandler
    {
        // Core properties for managing the Twitch connection
        private TwitchClient _Client { get; set; }
        /// <summary>Current WebSocket session identifier</summary>
        private string _SessionID { get; set; }
        /// <summary>List of active event subscriptions</summary>
        private List<TwitchEvent> _Events { get; set; }

        // WebSocket state management
        private WebsocketClient? _wsClient;
        private bool _isReconnecting = false;

        // Event delegates for different Twitch events
        public event Action<TwitchClient, ClientWhisperReceivedArgs>? OnClientWhisperReceived;
        /// <summary>Triggered when a chat message is received</summary>
        public event Action<TwitchClient, ChannelChatMessageReceivedArgs>? OnChannelChatMessageReceived;
        /// <summary>Triggered when a channel stream goes online</summary>
        public event Action<TwitchClient, ChannelStreamOnlineArgs>? OnChannelStreamOnline;
        /// <summary>Triggered when a channel stream goes offline</summary>
        public event Action<TwitchClient, ChannelStreamOfflineArgs>? OnChannelStreamOffline;
        /// <summary>Triggered when a channel receives a new follower</summary>
        public event Action<TwitchClient, ChannelFollowReceicvedArgs>? OnChannelFollowReceived;

        /// <summary>
        /// Initializes a new instance of the TwitchEventHandler with a WebSocket connection
        /// </summary>
        public TwitchEventHandler(TwitchClient client)
        {
            _Client = client;
            _SessionID = "null";
            CreateNewWsClient(new Uri("wss://eventsub.wss.twitch.tv/ws"));
            _Events = new();
        }

        /// <summary>
        /// Creates and configures a new WebSocket client with event handlers for various socket events
        /// </summary>
        private void CreateNewWsClient(Uri uri)
        {
            var newClient = new WebsocketClient(uri);

            // Handle incoming WebSocket messages
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
                        // Received when successfully connecting to EventSub
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
                        // Regular ping to maintain connection
                        TwitchSharpEngine.SendConsole("WS - Session Keepalive", TwitchSharpEngine.ConsoleLevel.Trace);
                        break;

                    case "session_reconnect":
                        // Server requesting client to reconnect to a new WebSocket URL
                        string reconnectUrl = payload.GetProperty("session").GetProperty("reconnect_url").GetString()!;
                        TwitchSharpEngine.SendConsole("WS - Reconnect requested, waiting for welcome before switching.", TwitchSharpEngine.ConsoleLevel.Debug);
                        CreateNewWsClient(new Uri(reconnectUrl));
                        break;

                    case "revocation":
                        // Event subscription has been revoked
                        var subscription = payload.GetProperty("subscription");
                        string type = subscription.GetProperty("type").GetString()!;
                        string status = subscription.GetProperty("status").GetString()!;
                        TwitchUser broadcaster = _Client.GetUserByIDAsync(subscription.GetProperty("condition").GetProperty("broadcaster_user_id").GetString()!).Result;
                        TwitchSharpEngine.SendConsole($"Event Subscription {type} for {broadcaster.DisplayName} ({broadcaster.ID}) was revoked and will no longer send events! Status: {status}", TwitchSharpEngine.ConsoleLevel.Warning);
                        break;

                    case "notification":
                        // New event notification received
                        HandleEvent(metadata, payload);
                        break;
                }
            });

            // Handle WebSocket disconnection
            newClient.DisconnectionHappened.Subscribe(info =>
            {
                if (!_isReconnecting)
                {
                    TwitchSharpEngine.SendConsole($"WS - Connection lost, attempting to reconnect... \nStatus: {info.CloseStatus} - {info.CloseStatusDescription}", TwitchSharpEngine.ConsoleLevel.Error);
                    _isReconnecting = true;
                    CreateNewWsClient(uri);
                }
            });

            // Handle WebSocket reconnection
            newClient.ReconnectionHappened.Subscribe(info =>
            {
                if (_isReconnecting)
                {
                    TwitchSharpEngine.SendConsole("WS - Reconnected, resubscribing to events...", TwitchSharpEngine.ConsoleLevel.Information);
                    ResubscribeToEventsAsync().Wait();
                    _isReconnecting = false;
                }
            });

            newClient.Start().Wait();
        }

        /// <summary>
        /// Processes incoming event notifications and triggers appropriate event handlers
        /// </summary>
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

        /// <summary>
        /// Subscribes to a specific Twitch event using the EventSub API
        /// </summary>
        /// <param name="twitchEvent">The event to subscribe to</param>
        /// <param name="isReconnect">Indicates if this is a resubscription after reconnect</param>
        public async Task SubscribeToEventAsync(TwitchEvent twitchEvent, bool isReconnect = false)
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

                if (result.IsSuccessStatusCode)
                {
                    if (!isReconnect)
                    {
                        _Events.Add(twitchEvent);
                        TwitchSharpEngine.SendConsole($"WS - Subscribed to event {twitchEvent.Type}", TwitchSharpEngine.ConsoleLevel.Debug);
                    }
                }
                else
                {
                    var body = await result.Content.ReadAsStringAsync();
                    TwitchSharpEngine.SendConsole($"WS - Failed to subscribe to event {twitchEvent.Type}: {result.StatusCode} - {body}", TwitchSharpEngine.ConsoleLevel.Error);
                }
            }
        }

        /// <summary>
        /// Resubscribes to all previously subscribed events after a reconnection
        /// </summary>
        private async Task ResubscribeToEventsAsync()
        {
            await Task.Delay(2000); // Wait for session to be fully ready
            foreach (var evt in _Events.ToList())
            {
                try
                {
                    await SubscribeToEventAsync(evt, true);
                    TwitchSharpEngine.SendConsole($"WS - Resubscribed to event {evt.Type}", TwitchSharpEngine.ConsoleLevel.Debug);
                }
                catch (Exception ex)
                {
                    TwitchSharpEngine.SendConsole($"WS - Failed to resubscribe to event {evt.Type}: {ex.Message}", TwitchSharpEngine.ConsoleLevel.Error);
                }
            }
        }
    }

    /// <summary>
    /// Base class for all Twitch event subscriptions
    /// </summary>
    public class TwitchEvent
    {
        /// <summary>The type of the Twitch event (e.g., "channel.follow")</summary>
        public string Type;
        /// <summary>API version for the event</summary>
        public int Version;
        /// <summary>Whether the event requires user authentication</summary>
        public bool RequiresUser;

        public TwitchEvent(string type, int version, bool requiresUser)
        {
            Type = type;
            Version = version;
            RequiresUser = requiresUser;
        }
    }

    /// <summary>
    /// Interface for events that are specific to a broadcaster's channel
    /// </summary>
    public interface IIsBroadcasterEvent
    {
        /// <summary>The broadcaster associated with this event</summary>
        public TwitchUser Broadcaster { get; }
        /// <summary>Indicates if the event requires moderator privileges</summary>
        public bool RequiresModeratorRole { get; }
    }
}