using DSharpPlus;
using DSharpPlus.EventArgs;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class ErrorEvents :
        IEventHandler<ClientErrorEventArgs>,
        IEventHandler<SocketErrorEventArgs>
    {
        public Task HandleEventAsync(DiscordClient s, ClientErrorEventArgs e)
        {
            SLogger.Log($"The Client ran into an Error ({e.EventName})", "ErrorEvents.cs -> ClientError", e.Exception, LogType.Error);
            return Task.CompletedTask;
        }

        public Task HandleEventAsync(DiscordClient s, SocketErrorEventArgs e)
        {
            SLogger.Log($"The Client-Socket ran into an Error", "ErrorEvents.cs -> SocketError", e.Exception, LogType.Error);
            return Task.CompletedTask;
        }
    }
}