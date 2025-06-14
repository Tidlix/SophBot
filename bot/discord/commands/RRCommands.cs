using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.discord.commands
{
    public class RRCommands
    {
        [Command("create_rr"), RequirePermissions(DiscordPermission.ManageRoles)]
        public async ValueTask createRR(CommandContext ctx,
            [Description("Rolle 1")] DiscordRole role1, [Description("Beschreibung für Rolle 1")] string role1Description,                  // Both always needed
            [Description("Rolle 2")] DiscordRole? role2 = null, [Description("Beschreibung für Rolle 2")] string? role2Description = null,  // When one is set, the other one has too 
            [Description("Rolle 3")] DiscordRole? role3 = null, [Description("Beschreibung für Rolle 3")] string? role3Description = null,  // When one is set, the other one has too
            [Description("Rolle 4")] DiscordRole? role4 = null, [Description("Beschreibung für Rolle 4")] string? role4Description = null,  // When one is set, the other one has too
            [Description("Rolle 5")] DiscordRole? role5 = null, [Description("Beschreibung für Rolle 5")] string? role5Description = null)  // When one is set, the other one has too
        {
            try
            {
                bool isRole2 = IsRoleSet([role2, role2Description]);
                bool isRole3 = IsRoleSet([role3, role3Description]);
                bool isRole4 = IsRoleSet([role4, role4Description]);
                bool isRole5 = IsRoleSet([role5, role5Description]);

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Rollenvergabe"));

                components.Add(new DiscordSeparatorComponent(true));
                components.Add(new DiscordSectionComponent(
                    new DiscordTextDisplayComponent($"{role1.Mention} -> {role1Description}"),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"rrSystem_role={role1.Id};", "Rolle ändern...")
                ));

                if (isRole2)
                {
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordSectionComponent(
                        new DiscordTextDisplayComponent($"{role2!.Mention} -> {role2Description}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"rrSystem_role={role2.Id};", "Rolle ändern...")
                    ));
                }
                if (isRole3)
                {
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordSectionComponent(
                        new DiscordTextDisplayComponent($"{role3!.Mention} -> {role3Description}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"rrSystem_role={role3.Id};", "Rolle ändern...")
                    ));
                }
                if (isRole4)
                {
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordSectionComponent(
                        new DiscordTextDisplayComponent($"{role4!.Mention} -> {role4Description}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"rrSystem_role={role4.Id};", "Rolle ändern...")
                    ));
                }
                if (isRole5)
                {
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordSectionComponent(
                        new DiscordTextDisplayComponent($"{role5!.Mention} -> {role5Description}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"rrSystem_role={role5.Id};", "Rolle ändern...")
                    ));
                }

                await ctx.DeferResponseAsync();
                await ctx.DeleteResponseAsync();

                DiscordChannel channel = ctx.Channel;
                await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components, false, role1.Color)));
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Warning, "Couldn't create ReactionRole message", "RRCommands.cs -> createRR()", ex);
                if (ex.Message == "Role group mismatch!")
                {
                    await ctx.RespondAsync("Ein Fehler ist aufgetreten! Bitte gib für jede Rolle eine Beschreibung an, sowie umgekehrt!");
                    return;
                }
                await ctx.RespondAsync("Ein Fehler ist aufgetreten! Bitte kontaktiere den Entwickler!");
            }

        }

        public bool IsRoleSet(object?[] objects)
        {
            bool isSet = false;

            foreach (var current in objects)
            {
                bool currentSet = (current == null) ? false : true;

                if (!isSet && currentSet) isSet = true;
                if (isSet && !currentSet)
                {
                    isSet = false;
                    throw new Exception("Role group mismatch!");
                }
            }

            return isSet;
        }
    }
}