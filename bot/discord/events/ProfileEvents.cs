using DSharpPlus;
using DSharpPlus.EventArgs;

namespace SophBot.bot.discord.events
{
    public class ProfileEvents :
        IEventHandler<MessageCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            if (e.Author.IsBot) return;
            if (e.Message.Content.StartsWith('!') || e.Message.Content.StartsWith('?')) return;

            SDiscordUser user = new(e.Author, e.Guild);
            await user.AddMessageCountAsnyc();

            var msgs = await user.GetMessageCountAsnyc(); // only full numbers 1,2,3,...

            if (msgs % 10 == 0) await user.AddPointsAsnyc(20); 
        }
    }
}