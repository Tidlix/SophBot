using DSharpPlus.Entities;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using System.ComponentModel;
using SophBot.Objects;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SophBot.Commands.ModCommands {
    public class ModerationCommands
    {
        [Command("warning"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public class WarnCommands
        {
            [Command("add"), Description("Verwarne einen Nutzer")]
            public async ValueTask warnUser(CommandContext ctx, DiscordMember target, string reason)
            {
                try
                {
                    TDiscordMember member = new(target);
                    await member.addWarningAsnyc(reason);
                    await ctx.RespondAsync($"Die Verwarnung für {target.Mention} wurde erstellt! Sieh dir alle Verwanungen mit /warnig get an");
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync(e.Message);
                }
            }

            [Command("get"), Description("Erhalte jede Verwarnung eines Nutzers")]
            public async ValueTask getWarning(SlashCommandContext ctx, [Description("Wähle den Nutzer, wessen Verwarnungen du erhalten möchtest")] DiscordMember target)
            {
                await ctx.DeferResponseAsync(true);
                try
                {
                    TDiscordMember member = new(target);
                    var warnings = await member.getWarningsAsnyc();

                    List<DiscordComponent> components = new();
                    components.Add(new DiscordTextDisplayComponent($"# Verwarnungen von {target.Mention}"));

                    int current = 0;
                    foreach (var warning in warnings)
                    {
                        if (current >= 10) break;
                        current++;
                        components.Add(new DiscordSectionComponent(
                            new DiscordTextDisplayComponent($"{warning.Date} - {warning.Reason}"),
                            new DiscordButtonComponent(DiscordButtonStyle.Danger, $"deleteUserWarningButton_{warning.WarnId}", "Verwarnung Aufheben")
                        ));
                    }

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components)));
                }
                catch (Exception e)
                {
                    TLog.sendLog(e.Message, TLog.MessageType.Error);
                    await ctx.EditResponseAsync("Etwas ist schiefgelaufen! - Bitte Kontaktiere den Entwickler dieses Bots");
                }
            }
        }

        [Command("SetPoints"), Description("Setzte die Punkte eines bestimmten Nutzers"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public async ValueTask setPoints(CommandContext ctx, DiscordMember target, ulong points)
        {
            TDiscordMember member = new(target);
            await ctx.DeferResponseAsync();

            await member.setGuildPointsAsnyc(points);

            await ctx.EditResponseAsync($"Du hast die Punkte von {target.Mention} auf {points} gesetzt!");
        }
    }
}