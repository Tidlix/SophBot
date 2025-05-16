using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.EventHandlers {
    public class ModalEvents : IEventHandler<ModalSubmittedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ModalSubmittedEventArgs e)
        {
            if (e.Id.Contains("commandModal_")) await customCommand(e);
            else {
                switch (e.Id) {
                    case "ruleModal":
                        await sendRules(e);
                        break;
                }
            }         
        }


        private async ValueTask sendRules(ModalSubmittedEventArgs e) {
            #pragma warning disable CS8602 


            await e.Interaction.DeferAsync(true);
            var values = e.Values.Values.ToArray();
            string title = values[0];
            string description = values[1];
            try {
                var channel = await e.Interaction.Guild.GetChannelAsync(await TidlixDB.ServerConfig.readValueAsync("rulechannel", e.Interaction.Guild.Id));
                var msg = await channel.SendMessageAsync(new DiscordEmbedBuilder() {
                    Title = title,
                    Description = description,
                    Color = DiscordColor.SpringGreen
                });
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Nachricht gesendet! {msg.JumpLink}"));

            } catch (Exception ex) {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Fehler - Regel konnte nicht gesendet werden! Bitte kontaktiere den Entwickler dieses Bots!"));
                await Log.sendMessage($"Rule channel from {e.Interaction.Guild.Name}({e.Interaction.Guild.Id}) couldn't be readed! - {ex.Message}", MessageType.Error());
            }
        }

        private async ValueTask customCommand(ModalSubmittedEventArgs e)
        {
            await e.Interaction.DeferAsync(true);
            string command = e.Id.Substring(e.Id.IndexOf('_') + 1);
            string value = e.Values.Values.First();
            ulong guildId = e.Interaction.Guild.Id;

            if (await TidlixDB.CustomCommands.checkExistanceAsync(command, guildId))
            {
                if (value == "")
                {
                    await TidlixDB.CustomCommands.deleteAsnyc(command, guildId);
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde gel�scht!"));
                }
                else
                {
                    await TidlixDB.CustomCommands.modifyAsync(command, value, guildId);
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde bearbeitet!"));
                }
            }
            else
            {
                if (value != "")
                {
                    await TidlixDB.CustomCommands.createAsnyc(command, value, guildId);
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde erstellt!"));
                }
                else await e.Interaction.DeleteOriginalResponseAsync();
            }
        }
    }
}