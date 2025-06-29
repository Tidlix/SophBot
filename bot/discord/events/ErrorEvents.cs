using DSharpPlus;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class ErrorEvents :
        IEventHandler<ClientErrorEventArgs>,
        IEventHandler<SocketErrorEventArgs>
    {
        public Task HandleEventAsync(DiscordClient s, ClientErrorEventArgs e)
        {
            SLogger.Log(LogLevel.Error, $"The Client ran into an Error ({e.EventName})", "ErrorEvents.cs", e.Exception);
            return Task.CompletedTask;
        }

        public Task HandleEventAsync(DiscordClient s, SocketErrorEventArgs e)
        {
            SLogger.Log(LogLevel.Error, $"The Client-Socket ran into an Error", "ErrorEvents.cs", e.Exception);
            return Task.CompletedTask;
        }
    }
}