using SophBot.Universal;
using TwitchSharp;
using TwitchSharp.Events.Types;

namespace SophBot.Twitch.EventHandlers
{
    public static class ChannelChatMessageReceivedHandler
    {
        public static async Task OnReceive (TwitchClient s, ChannelChatMessageReceivedArgs e)
        {
            if (e.Chatter == s.CurrentUser) return;

            Profile user = new Profile(e.Chatter.ID);
            user.AddTwitchMessage();
            if (user.DiscordMessages + user.TwitchMessages % 10 == 0)
                    user.AddChannelpoints(50);
        }
    }
}