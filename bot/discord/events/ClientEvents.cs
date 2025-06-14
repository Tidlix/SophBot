using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class ClientEvents :
        IEventHandler<ClientStartedEventArgs>
    {
        public Task HandleEventAsync(DiscordClient s, ClientStartedEventArgs e)
        {
            SLogger.Log(LogLevel.Information, "Bot Started and Ready!", "ClientEvents.cs");
            return Task.CompletedTask;
        }
    }
}