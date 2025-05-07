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
            if (e.Id.Contains("createCommand")) await createCommand(e);
            else if (e.Id.Contains("modifyCommand")) await modifyCommand(e);
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
                var channel = await e.Interaction.Guild.GetChannelAsync(await TidlixDB.readServerconfig("rulechannel", e.Interaction.Guild.Id));
                var msg = await channel.SendMessageAsync(new DiscordEmbedBuilder() {
                    Title = title,
                    Description = description,
                    Color = DiscordColor.SpringGreen
                });
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Nachricht gesendet! {msg.JumpLink}"));

            } catch (Exception ex) {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Fehler - Regel konnte nicht gesendet werden! Bitte kontaktiere den Entwickler dieses Bots!"));
                await MessageSystem.sendMessage($"Rule channel from {e.Interaction.Guild.Name}({e.Interaction.Guild.Id}) couldn't be readed! - {ex.Message}", MessageType.Error());
            }
        }
    
        private async ValueTask createCommand(ModalSubmittedEventArgs e) {
            await e.Interaction.DeferAsync(true);
            string command = e.Id.Replace("createCommand", "");

            try {
                await TidlixDB.createCommand(command, e.Values.Values.First(), e.Interaction.Guild.Id);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde erfolgreich erstellt!"));
            } catch (Exception ex) {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Fehler - Der Command konnte nicht erstellt werden! Bitte kontaktiere den Entwickler dieses Bots!"));
                await MessageSystem.sendMessage($"Command for {e.Interaction.Guild.Name}({e.Interaction.Guild.Id}) couldn't be created! - {ex.Message}", MessageType.Error());
            }
        }

        private async ValueTask modifyCommand(ModalSubmittedEventArgs e) {
            await e.Interaction.DeferAsync(true);
            string command = e.Id.Replace("modifyCommand", "");

            try {
                await TidlixDB.modifyCommand(command, e.Values.Values.First(), e.Interaction.Guild.Id);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde erfolgreich bearbeitet!"));
            } catch (Exception ex) {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Fehler - Command konnte nicht bearbeitet werden! Bitte kontaktiere den Entwickler dieses Bots!"));
                await MessageSystem.sendMessage($"Command for {e.Interaction.Guild.Name}({e.Interaction.Guild.Id}) couldn't be created! - {ex.Message}", MessageType.Error());
            }
        }
    }
}