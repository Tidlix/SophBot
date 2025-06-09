using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using SophBot.bot.conf;
using SophBot.bot.logs;

namespace SophBot.bot.discord.commands
{
    [Command("debug"), RequireApplicationOwner, AllowedProcessors<TextCommandProcessor>]
    public class DebugCommands
    {
        [Command("loglevel"), Description("Change the current LogLevel")]
        public async ValueTask changeLogLevel(CommandContext ctx, LogType LogLevel)
        {
            SConfig.LogLevel = LogLevel;
            await ctx.RespondAsync($"LogLevel changed to `{LogLevel}`");
        }
    }
}