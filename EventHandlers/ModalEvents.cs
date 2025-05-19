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
                switch (e.Id)
                {
                    case "ruleModal":
                        await sendRules(e);
                        break;
                    case "suggestionModal":
                        await sendSuggestion(s, e);
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

        private async ValueTask sendSuggestion(DiscordClient s, ModalSubmittedEventArgs e)
        {
#pragma warning disable CS8604
            await e.Interaction.DeferAsync(true);

            var owner = s.CurrentApplication.Owners.First();
            var components = new DiscordComponent[] {
                new DiscordTextDisplayComponent($"### {e.Interaction.User.Mention} has sent an Suggestion:"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{e.Values.Values.First()}")
            };
            await owner.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, false, DiscordColor.Azure)));

            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Deine Idee wurde weitergegeben! \nBitte beachte dass es ein wenig dauern kann, bis deine Idee angeschaut wird. *Ggf. könnten in den nächsten Tagen weitere Nachfragen auf dich zu kommen, sollte etwas nicht ganz klar sein.*"));
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