using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands
{
    [Command("Warning"), RequirePermissions(DiscordPermission.ModerateMembers)]
    public class WarningCommands
    {
        [Command("Add"), Description("Verwarne einen Nutzer")]
        public async ValueTask addWarning(CommandContext ctx, DiscordMember member, string reason)
        {
            await ctx.DeferResponseAsync();
            SDiscordUser user = new(member);

            await user.AddWarningAsnyc(reason);
            await ctx.EditResponseAsync($"Die Verwarnung für {member.Mention} wurde erstellt!\n > {reason}\n> {DateTime.Now.ToString("dd.MM.yyyy - HH:mm")}");
        }
        [Command("Get"), Description("Erhalte eine Lister aller Verwarnungen eines Nutzers")]
        public async ValueTask getWarnings(CommandContext ctx, DiscordMember member)
        {
            await ctx.DeferResponseAsync();
            SDiscordUser user = new(member);
            Dictionary<string, string> list = await user.GetWarningsAsync();

            string response = $"Hier sind alle Verwarnungen von {member.Mention}:";

            foreach (var current in list)
            {
                response += $"\n* **{current.Key}** ~ {current.Value}";
            }
            await ctx.EditResponseAsync(response);
        }
    }
}