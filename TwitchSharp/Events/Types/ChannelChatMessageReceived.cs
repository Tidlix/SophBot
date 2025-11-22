using System.Text.Json;
using TwitchSharp.Entities;

namespace TwitchSharp.Events.Types
{
    /// <summary>
    /// Event subscription for channel chat messages
    /// </summary>
    public class ChannelChatMessageReceivedEvent(TwitchUser broadcaster) : TwitchEvent("channel.chat.message", 1, true), IIsBroadcasterEvent
    {
        TwitchUser IIsBroadcasterEvent.Broadcaster => broadcaster;
        bool IIsBroadcasterEvent.RequiresModeratorRole => false;
    }

    /// <summary>
    /// Arguments containing chat message data
    /// </summary>
    public class ChannelChatMessageReceivedArgs
    {
        /// <summary>The content of the chat message</summary>
        public string MessageContent { get; private set; }
        /// <summary>Unique identifier for the message</summary>
        public string MessageID { get; private set; }
        /// <summary>Channel owner where message was sent</summary>
        public TwitchUser Broadcaster { get; private set; }
        /// <summary>User who sent the message</summary>
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