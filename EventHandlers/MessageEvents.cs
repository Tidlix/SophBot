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
            string msg = e.Message.Content.Substring(e.Message.Content.IndexOf('!'));
            string command = (msg.Contains(' ')) ? msg.Substring(0, msg.IndexOf(' ')).ToLower() : msg.ToLower();
            string text = msg.Substring(msg.IndexOf(' ')+1);

            try {
                if (!await TidlixDB.checkCommandExists(command, e.Guild.Id)) return;
                string response = await TidlixDB.getCommand(command, e.Guild.Id);

                /*
                [rand(###)] -> new Random().Next(###).ToString();
                -> Zahl in Klammern() wird automatisch erkannt
                -> Zahl soll automatisch als max. Wert angegeben werden
                -> Sollten mehrere [rand(###)] variablen im String verwendet sein, wird jede variable einzeln getauscht
                z.B.:
                [rand(100)] -> Zahl 1-100
                [rand(50)] -> Zahl 1-50
                */
                if (response.Contains("[rand100]")) response = response.Replace("[rand100]", new Random().Next(100).ToString());


                if (response.Contains("[user]")) response = response.Replace("[user]", e.Author.Mention);
                if (response.Contains("[text]")) response = response.Replace("[text]", text);
                if (response.Contains("[first]")) response = response.Replace("[first]", (text.Contains(' ')) ? text.Substring(0, text.IndexOf(' ')) : text);

                await e.Message.RespondAsync(response);
            } catch (Exception ex) {
                await MessageSystem.sendMessage($"Failed to try customcommand - {ex.Message}", MessageType.Warning());
            }
        }
    }
}