using DSharpPlus;
using DSharpPlus.EventArgs;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.EventHandlers {
    class MessageEvents : IEventHandler<MessageCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            if (e.Message.Content.StartsWith("!")) await customCommand(e);
        }


        public async ValueTask customCommand(MessageCreatedEventArgs e) {
            string msg = e.Message.Content.TrimStart('!');
            string command = (msg.Contains(' ')) ? msg.Substring(0, msg.IndexOf(' ')).ToLower() : msg.ToLower();
            string text = msg.Substring(msg.IndexOf(' ')+1);

            try {
                if (!await TidlixDB.checkCommandExists(command, e.Guild.Id)) return;
                string response = await TidlixDB.getCommand(command, e.Guild.Id);

                if (response.Contains("[user]")) response = response.Replace("[user]", e.Author.Mention);
                if (response.Contains("[rand100]")) response = response.Replace("[rand100]", new Random().Next(100).ToString());
                if (response.Contains("[text]")) response = response.Replace("[text]", text);
                if (response.Contains("[first]")) response = response.Replace("[first]", (text.Contains(' ')) ? text.Substring(0, text.IndexOf(' ')) : text);

                await e.Message.RespondAsync(response);
            } catch (Exception ex) {
                await MessageSystem.sendMessage($"Failed to try customcommand - {ex.Message}", MessageType.Warning());
            }
        }
    }
}