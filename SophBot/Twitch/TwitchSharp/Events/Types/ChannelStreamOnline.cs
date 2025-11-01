using System.Text.Json;
using TwitchSharp.Entitys;

namespace TwitchSharp.Events.Types
{
    public class ChannelStreamOnlineEvent(TwitchUser broadcaster) : TwitchEvent("stream.online", 1, false), IIsBroadcasterEvent
    {
        TwitchUser IIsBroadcasterEvent.Broadcaster => broadcaster;
        bool IIsBroadcasterEvent.RequiresModeratorRole => false;
    }
    public class ChannelStreamOnlineArgs
    {
        public TwitchUser Broadcaster { get; private set; }
        public string StreamID { get; private set; }
        public string StreamType { get; private set; }
        public string StartedAt { get; private set; }
        public TwitchStream Stream { get; private set; }
        public ChannelStreamOnlineArgs(TwitchClient client, JsonElement payload)
        {
            var Event = payload.GetProperty("event");
            Broadcaster = client.GetUserByIDAsync(Event.GetProperty("broadcaster_user_id").GetString()!).Result;
            StreamID = Event.GetProperty("id").GetString()!;
            StreamType = Event.GetProperty("type").GetString()!;
            StartedAt = Event.GetProperty("started_at").GetString()!;
            Stream = Broadcaster.GetCurrentStream()!;
        }
    }
}