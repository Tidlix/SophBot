using TwitchLib.Client.Events;

namespace SophBot.Twitch.Commands
{
    public static class CommandHandler
    {
        public static async Task OnCommandSend(object? sender, OnChatCommandReceivedArgs args)
        {
            switch (args.Command.Name.ToLower())
            {
                case "ai": await AICommands.HandleCommandAsync(sender, args); break;
            } 
        }
    }
}