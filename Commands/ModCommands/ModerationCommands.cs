using DSharpPlus.Entities;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using System.ComponentModel;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.ModCommands {
    public class ModerationCommands
    {
        [Command("warning"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public class WarnCommands
        {
            [Command("add"), Description("Verwarne einen Nutzer")]
            public async ValueTask warnUser(CommandContext ctx, DiscordMember member, string reason)
            {
#pragma warning disable CS8602
                try
                {
                    await TidlixDB.Warnings.createAsnyc(ctx.Guild.Id, member.Id, reason);
                    await ctx.RespondAsync($"Die Verwarnung für {member.Mention} wurde erstellt! Sieh dir alle Verwanungen mit /warnig get an");
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("Die Verwarnung konnte nicht ausgeführt werden! Bitte kontaktiere den Entwickler dieses Bots!");
                    await Log.sendMessage($"Couldn't warn user {member.DisplayName}({member.Id}) - {e.Message}", MessageType.Error());
                }
            }
            [Command("remove"), Description("Entferne eine Verwarnung eines Nutzers")]
            public async ValueTask deleteWarning(CommandContext ctx, [Description("Warn-ID der Verwarnung die du löschen willst")] ulong warnid)
            {
                try
                {
                    await TidlixDB.Warnings.deleteAsnyc(warnid, ctx.Guild.Id);
                    await ctx.RespondAsync($"Die Verwarnung #{warnid} wurde gelöscht!");
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("Die Löschung konnte nicht ausgeführt werden! Bitte kontaktiere den Entwickler dieses Bots!");
                    await Log.sendMessage($"Couldn't delete warning {warnid} - {e.Message}", MessageType.Error());
                }
            }
            [Command("get"), Description("Erhalte jede Verwarnung eines Nutzers")]
            public async ValueTask getWarning(CommandContext ctx, DiscordMember member)
            {
                try
                {
                    string warnings = await TidlixDB.Warnings.getAllByUserAsnyc(member.Id, ctx.Guild.Id);
                    await ctx.RespondAsync($"**Hier sind alle Verwarnungen von {member.Mention}:**\n\n" + warnings);
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("Die Verwarnungen konnten nich gesammelt werden! Bitte kontaktiere den Entwickler dieses Bots!");
                    await Log.sendMessage($"select warnings from {member.DisplayName}({member.Id}) > Guild {ctx.Guild.Name}({ctx.Guild.Id}) - {e.Message}", MessageType.Error());
                }
            }
        }

        [Command("SetPoints"), Description("Setzte die Punkte eines bestimmten Nutzers"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public async ValueTask setPoints(CommandContext ctx, DiscordMember member, ulong points)
        {
            await ctx.DeferResponseAsync();

            await TidlixDB.UserProfiles.modifyValueAsnyc("points", points.ToString(), ctx.Guild.Id, member.Id);

            await ctx.EditResponseAsync($"Du hast die Punkte von {member.Mention} auf {points} gesetzt!");
        }
    }
}