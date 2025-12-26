using SophBot.Universal;
using TwitchLib.Client.Events;

namespace SophBot.Twitch.Events
{
    public class MessageEvents
    {
        public static async Task OnMessageReceived (object? s, OnMessageReceivedArgs args)
        {
            if (args.ChatMessage.IsMe) return;
            
            Profile profile = new Profile(args.ChatMessage.UserId);
            profile.AddTwitchMessage();
            if (profile.DiscordMessages + profile.TwitchMessages % 10 == 0)
                profile.AddChannelpoints(50);

            await Task.Delay(1); // Temp fix for warning CS1998
        }
    }
}