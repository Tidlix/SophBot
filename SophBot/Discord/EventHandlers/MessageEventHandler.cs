using DSharpPlus;
using DSharpPlus.EventArgs;
using SophBot.Universal;

namespace SophBot.Discord.EventHandlers
{
    public class MessageEventHandler : IEventHandler<MessageCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            if (e.Author.IsBot) return;

            if(e.Channel.IsPrivate)
            {
                string response = await GeminiEngine.GenerateResponseAsync(new DiscordAiRequest(e.Channel, e.Author, e.Message.Content));
                await e.Channel.SendMessageAsync(response);
            }
            else
            {
                Profile user = new Profile(e.Author.Id);
                user.AddDiscordMessage();
                if (user.DiscordMessages + user.TwitchMessages % 10 == 0)
                    user.AddChannelpoints(50);
            }
        }
    }
}