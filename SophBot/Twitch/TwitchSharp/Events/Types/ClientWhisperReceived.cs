using System.Text.Json;
using TwitchSharp.Entitys;

namespace TwitchSharp.Events.Types
{
    public class ClientWhisperReceivedEvent() : TwitchEvent("user.whisper.message", 1, true)
    {

    }

    public class ClientWhisperReceivedArgs
    {
        public string MessageContent { get; private set; }
        public string MessageID { get; private set; }
        public TwitchUser Sender { get; private set; }
        public TwitchUser Receiver { get; private set; }

        public ClientWhisperReceivedArgs(TwitchClient client, JsonElement payload)
        {
            var _Event = payload.GetProperty("event");
            Sender = client.GetUserByLoginAsync(_Event.GetProperty("from_user_login").GetString()!).Result;
            Receiver = client.GetUserByLoginAsync(_Event.GetProperty("to_user_login").GetString()!).Result;
            MessageID = _Event.GetProperty("whisper_id").GetString()!;
            MessageContent = _Event.GetProperty("whisper").GetProperty("text").GetString()!;
        }
    }
}