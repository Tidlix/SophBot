using SophBot.Universal;
using TwitchSharp;
using TwitchSharp.Events.Types;

namespace SophBot.Twitch.EventHandlers
{
    public static class ChannelChatMessageReceivedHandler
    {
        #pragma warning disable CS1998
        public static async Task OnReceive (TwitchClient s, ChannelChatMessageReceivedArgs e)
        {
            if (e.Chatter == s.CurrentUser) return;

            Profile user = new Profile(e.Chatter.ID);
            user.AddTwitchMessage();
            if (user.DiscordMessages + user.TwitchMessages % 10 == 0)
                    user.AddChannelpoints(50);

            if (e.MessageContent.ToLower().StartsWith("!ai ") 
            || e.MessageContent.ToLower().StartsWith($"@{s.CurrentUser.LoginName}"))
            {
                string response = await GeminiEngine.GenerateResponseAsync(new TwitchAiRequest(e.Broadcaster.DisplayName, false, e.Chatter, e.MessageContent));
                await e.Broadcaster.SendChatMessageAsync(response, e.MessageID);
            }
        }
        #pragma warning restore CS1998
    }
}