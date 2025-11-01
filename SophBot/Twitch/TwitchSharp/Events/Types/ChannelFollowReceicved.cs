using System.Text.Json;
using TwitchSharp.Entitys;

namespace TwitchSharp.Events.Types
{
    public class ChannelFollowReceivedEvent(TwitchUser broadcaster) : TwitchEvent("channel.follow", 2, true), IIsBroadcasterEvent
    {
        TwitchUser IIsBroadcasterEvent.Broadcaster => broadcaster;
        bool IIsBroadcasterEvent.RequiresModeratorRole => true;
    }

    public class ChannelFollowReceicvedArgs
    {
        public TwitchUser Broadcaster { get; private set; }
        public TwitchUser Follower { get; private set; }
        public DateTime FollowedAt { get; private set; }

        public ChannelFollowReceicvedArgs(TwitchClient client, JsonElement payload)
        {
            var _Event = payload.GetProperty("event");

            Broadcaster = client.GetUserByLoginAsync(_Event.GetProperty("broadcaster_user_login").GetString()!).Result;
            Follower = client.GetUserByLoginAsync(_Event.GetProperty("user_login").GetString()!).Result;

            FollowedAt = _Event.GetProperty("followed_at").GetDateTime();
        }
    }
}