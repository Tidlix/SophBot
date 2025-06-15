using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands
{
    [Command("RuleSystem"), RequirePermissions(DiscordPermission.Administrator)]
    public class RuleCommands {
        [Command("sendrule"), Description("Sende eine Regel im Konfigurierten Regel-Kanal")]
        public async ValueTask sendRules(SlashCommandContext ctx)
        {
                DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder() {
                Title = "Regel-Erstellung...",
                CustomId = "ruleSystem-sendRule"
                };

                modal.AddTextInputComponent(new DiscordTextInputComponent(label: $"Titel:", customId: $"ruleTitle", max_length: 150, style: DiscordTextInputStyle.Short));
                modal.AddTextInputComponent(new DiscordTextInputComponent(label: $"Beschreibung:", customId: $"ruleDescription", max_length: 2000, style: DiscordTextInputStyle.Paragraph));

                await ctx.RespondWithModalAsync(modal);
        }
        
        [Command("sendAccept"), Description("Sende die Nachricht, mit welcher die Regeln bestätigt werden können")]
        public async ValueTask sendRuleAccept(SlashCommandContext ctx)
        {
            await ctx.DeferResponseAsync(true);
            SDiscordServer server = new(ctx.Guild!);

            DiscordComponent[] components = {
                new DiscordTextDisplayComponent($"### Regelbestätigung"),
                new DiscordSeparatorComponent(),
                new DiscordSectionComponent(
                    new DiscordTextDisplayComponent($"Hiermit bestätige ich, dass ich die Regeln gelesen habe, akzeptiere und mich somit auch daran halten werde."),
                    new DiscordButtonComponent(
                        style: DiscordButtonStyle.Success,
                        customId: "ruleSystem-acceptRule",
                        label: "Regeln Akzeptieren",
                        emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(client: ctx.Client,name: ":white_check_mark:"))
                ))
            };

            try {
                var channel = await server.getChannelAsync(SDiscordChannel.RuleChannel);
                var msg = await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.SpringGreen)));
                await ctx.EditResponseAsync($"Nachricht gesendet! {msg.JumpLink}");
            } catch (Exception ex) {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message));
            }
        }
    }
}