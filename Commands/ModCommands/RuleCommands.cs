using DSharpPlus.Entities;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using System.ComponentModel;
using DSharpPlus.Commands.Processors.SlashCommands;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.ModCommands {
    [Command("RuleSystem"), RequirePermissions(DiscordPermission.Administrator)]
    public class RuleCommands {
        [Command("sendrule"), Description("Sende eine Regel im Konfigurierten Regel-Kanal")]
        public async ValueTask sendRules(SlashCommandContext ctx) {
                DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder() {
                Title = "Regel-Erstellung...",
                CustomId = "ruleModal"
                };

                modal.AddComponents(new DiscordTextInputComponent(label: $"Titel:", customId: $"ruleTitle", max_length: 150, style: DiscordTextInputStyle.Short));
                modal.AddComponents(new DiscordTextInputComponent(label: $"Beschreibung:", customId: $"ruleDescription", max_length: 1000, style: DiscordTextInputStyle.Paragraph));

                await ctx.RespondWithModalAsync(modal);
        }
        [Command("sendAccept"), Description("Sende die Nachricht, mit welcher die Regeln bestätigt werden können")]
        public async ValueTask sendRuleAccept(SlashCommandContext ctx) {
            #pragma warning disable CS8602 
            await ctx.DeferResponseAsync(true);

            DiscordMessageBuilder message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder() {
                Title = "Regelbestätigung",
                Description = "Hiermit bestätige ich, dass ich die Regeln gelesen habe und akzeptiere...",
                Color = DiscordColor.SpringGreen
            })
            .AddComponents(new DiscordButtonComponent(
                style: DiscordButtonStyle.Success,
                customId: "ruleAcceptButton",
                label: "Regeln Akzeptieren",
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(client: ctx.Client,name: ":white_check_mark:"))
            ));

            try {
                var channel = await ctx.Guild.GetChannelAsync(await TidlixDB.readServerconfig("rulechannel", ctx.Guild.Id));
                var msg = await channel.SendMessageAsync(message);
                await ctx.EditResponseAsync($"Nachricht gesendet! {msg.JumpLink}");
            } catch (Exception ex) {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Fehler - Regel konnte nicht gesendet werden! Bitte kontaktiere den Entwickler dieses Bots!"));
                await Log.sendMessage($"Rule channel from {ctx.Guild.Name}({ctx.Guild.Id}) couldn't be readed! - {ex.Message}", MessageType.Error());
            }
        }
    }
}