using System.Text.Json;
using TwitchSharp.Entitys;

namespace TwitchSharp.Events.Types
{
    public class ChannelChatMessageReceivedEvent(TwitchUser broadcaster) : TwitchEvent("channel.chat.message", 1, true), IIsBroadcasterEvent
    {
        TwitchUser IIsBroadcasterEvent.Broadcaster => broadcaster;
        bool IIsBroadcasterEvent.RequiresModeratorRole => false;
    }

    public class ChannelChatMessageReceivedArgs
    {
        public string MessageContent { get; private set; }
        public string MessageID { get; private set; }
        public TwitchUser Broadcaster { get; private set; }
        public TwitchUser Chatter { get; private set; }

        public ChannelChatMessageReceivedArgs(TwitchClient client, JsonElement payload)
        {
            var _Event = payload.GetProperty("event");

            Chatter = client.GetUserByLoginAsync(_Event.GetProperty("chatter_user_login").GetString()!).Result;
            Broadcaster = client.GetUserByLoginAsync(_Event.GetProperty("broadcaster_user_login").GetString()!).Result;
            MessageID = _Event.GetProperty("message_id").GetString()!;
            var _Message = _Event.GetProperty("message");
            MessageContent = _Message.GetProperty("text").GetString()!;
        }
    }
}