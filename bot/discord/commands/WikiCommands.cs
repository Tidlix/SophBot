using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.bot.discord.features;
using SophBot.bot.logs;

namespace SophBot.bot.discord.commands
{
    public class WikiCommands
    {
        [Command("Wiki"), Description("Öffne das SophWiki")]
        public async ValueTask openWiki(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            var msg = await WikiEngine.getWikiMessage("Startseite", 0);
            
            await ctx.EditResponseAsync(msg);
        }
    }
}