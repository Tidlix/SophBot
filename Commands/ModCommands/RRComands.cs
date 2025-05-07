using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace SophBot.Commands.ModCommands {
    public class RRCommands {
        [Command("createrr"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public async Task createRR(CommandContext ctx, DiscordRole role, [Description("Worfür ist diese Rolle gedacht")]string description) {
            await ctx.DeferResponseAsync();
            await ctx.DeleteResponseAsync();


            DiscordComponent[] components = [
                new DiscordTextDisplayComponent($"## Rollenvergabe - {role.Mention}"),
                new DiscordTextDisplayComponent($"**{description}**"),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent("Rolle erhalten/enfernen -> "), new DiscordButtonComponent(DiscordButtonStyle.Secondary, label: "Drück mich!", customId: $"rrButton_{role.Id}"))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddRawComponents(new DiscordContainerComponent(components, color: role.Color));

            await ctx.Channel.SendMessageAsync(msg);

            /*
            var msgBuilder = new DiscordMessageBuilder();

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Reactionrole",
                Description = "Drücke den Knopf um folgende Rolle zu erhalten:" +
                $"\n\n{role.Mention}" +
                $"\n{description}" +
                "\n\n*Drücke den Knopf erneut um die Rolle zu entfernen!*",
                Color = DiscordColor.Lilac,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = role.Id.ToString() }
            };
            var button = new DiscordButtonComponent(customId: "rrButton", label: "Drück mich!", style: DiscordButtonStyle.Primary);

            msgBuilder.AddEmbed(embed);
            msgBuilder.AddComponents(button);

            await ctx.Channel.SendMessageAsync(msgBuilder);
            */
        }
    }
}