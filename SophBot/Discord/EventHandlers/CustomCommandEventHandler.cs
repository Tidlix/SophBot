using DSharpPlus;
using DSharpPlus.EventArgs;
using SophBot.Discord.Tools;

namespace SophBot.Discord.EventHandlers
{
    public class CustomCommandEventHandler : IEventHandler<MessageCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
        {
            string content = eventArgs.Message.Content;
            if(!content.StartsWith('!')) return;
            content = content.TrimStart('!');
            string[] param = content.Split(' ');
            CustomCommand? command = CustomCommandEngine.getCommand(param[0]);
            if (command is null) return;
            await eventArgs.Message.RespondAsync(command.ToString(param, eventArgs.Author.Mention));
        }
    }
}