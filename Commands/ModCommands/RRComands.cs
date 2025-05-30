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
            .AddContainerComponent(new DiscordContainerComponent(components, color: role.Color));

            await ctx.Channel.SendMessageAsync(msg);
        }
    }
}