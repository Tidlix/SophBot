using System.Text.Json;
using TwitchSharp.Entities;

namespace TwitchSharp.Events.Types
{
    public class ChannelStreamOfflineEvent(TwitchUser broadcaster) : TwitchEvent("stream.offline", 1, false), IIsBroadcasterEvent
    {
        TwitchUser IIsBroadcasterEvent.Broadcaster => broadcaster;
        bool IIsBroadcasterEvent.RequiresModeratorRole => false;
    }
    public class ChannelStreamOfflineArgs
    {
        public TwitchUser Broadcaster { get; private set; }
        public ChannelStreamOfflineArgs(TwitchClient client, JsonElement payload)
        {
            Broadcaster = client.GetUserByIDAsync(payload.GetProperty("event").GetProperty("broadcaster_user_id").GetString()!).Result;
        }
    }
}