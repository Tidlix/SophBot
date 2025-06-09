using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.bot.discord.features.wiki;
using SophBot.bot.logs;

namespace SophBot.bot.discord.commands
{
    public class WikiCommands
    {
        [Command("Wiki"), Description("Öffne das SophWiki")]
        public async ValueTask openWiki(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            var msg = await WikiEngine.getWikiMessage("Test1", 1);
            
            await ctx.EditResponseAsync(msg);
        }
    }
}