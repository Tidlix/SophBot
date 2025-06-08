using DSharpPlus;
using DSharpPlus.EventArgs;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class ClientEvents : IEventHandler<ClientStartedEventArgs>
    {
        public Task HandleEventAsync(DiscordClient s, ClientStartedEventArgs e)
        {
            SLogger.Log("Bot Started and Ready!", type: LogType.Message);
            return Task.CompletedTask;
        }
    }
}