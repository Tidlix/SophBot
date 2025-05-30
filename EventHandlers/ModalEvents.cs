using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Objects;

namespace SophBot.EventHandlers {
    public class ModalEvents : IEventHandler<ModalSubmittedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ModalSubmittedEventArgs e)
        {
            if (e.Id.Contains("commandModal_")) await customCommand(e);
            else
            {
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


        private async ValueTask sendRules(ModalSubmittedEventArgs e)
        {
            await e.Interaction.DeferAsync(true);


            var values = e.Values.Values.ToArray();
            string title = values[0];
            string description = values[1];

            TDiscordGuild guild = new(e.Interaction.Guild!);
            try
            {
                var channel = await guild.getChannelAsnyc(TDiscordGuild.Channel.Rules);

                DiscordComponent[] components = {
                new DiscordTextDisplayComponent($"### {title}"),
                new DiscordSeparatorComponent(),
                new DiscordTextDisplayComponent($"{description}")
            };

                var msg = await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, false, DiscordColor.SpringGreen)));
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Regel gesendet! {msg.JumpLink}"));

            }
            catch (Exception ex)
            {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message));
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
            await owner.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, false, DiscordColor.Azure)));

            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Deine Idee wurde weitergegeben! \nBitte beachte dass es ein wenig dauern kann, bis deine Idee angeschaut wird. *Ggf. könnten in den nächsten Tagen weitere Nachfragen auf dich zu kommen, sollte etwas nicht ganz klar sein.*"));
        }

        private async ValueTask customCommand(ModalSubmittedEventArgs e)
        {
            await e.Interaction.DeferAsync(true);
            string command = e.Id.Substring(e.Id.IndexOf('_') + 1);
            string value = e.Values.Values.First();
            TDiscordGuild guild = new(e.Interaction.Guild);
            try
            {
                if (!(await guild.getCommandAsnyc(command) == null))
                {
                    if (value == "")
                    {
                        await guild.deleteCommandAsync(command);
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde gelöscht!"));
                    }
                    else
                    {
                        await guild.modifyCommandAsnyc(command, value);
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde bearbeitet!"));
                    }
                }
                else
                {
                    if (value != "")
                    {
                        await guild.createCommandAsnyc(command, value);
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde erstellt!"));
                    }
                    else await e.Interaction.DeleteOriginalResponseAsync();
                }
            }
            catch (Exception ex)
            {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Bei diesem Command ist ein Fehler aufgetreten! Bitte kontaktiere den Entwickler dieses Bots!"));
                TLog.sendLog($"Couldn't access Custom Command > {ex.Message} <", TLog.MessageType.Error);
            }
        }
    }
}