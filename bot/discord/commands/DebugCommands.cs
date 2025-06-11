using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using SophBot.bot.conf;
using SophBot.bot.logs;

namespace SophBot.bot.discord.commands
{
    [Command("debug"), RequireApplicationOwner,  RequireGuild]
    public class DebugCommands
    {
        [Command("serverconfig")]
        public class Serverconfig
        {
            [Command("Create")]
            public async ValueTask createSC(CommandContext ctx, DiscordChannel welcomeChannel, DiscordChannel ruleChannel, DiscordRole memberRole)
            {
                SDiscordServer Server = new(ctx.Guild!);
                await Server.createConfigAsync(welcomeChannel, ruleChannel, memberRole);
                await ctx.RespondAsync($"Server config created!");
            }
            [Command("Modify"), AllowedProcessors<SlashCommandProcessor>]
            public async ValueTask modifySC(CommandContext ctx, DiscordChannel? welcomeChannel = null, DiscordChannel? ruleChannel = null, DiscordRole? memberRole = null)
            {
                SDiscordServer Server = new(ctx.Guild!);
                try
                {
                    await Server.modifyConfigAsync(welcomeChannel, ruleChannel, memberRole);
                    await ctx.RespondAsync($"Server config modified!");
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Failed to modify Config: " + ex.Message);
                }
                
            }    
        }

        [Command("loglevel"), Description("Change the current LogLevel")]
        public async ValueTask changeLogLevel(CommandContext ctx, LogType LogLevel)
        {
            SConfig.LogLevel = LogLevel;
            await ctx.RespondAsync($"LogLevel changed to `{LogLevel}`");
        }
    }
}