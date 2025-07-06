using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.conf;
using SophBot.bot.logs;
using SophBot.bot.twitch;

namespace SophBot.bot.discord.commands
{
    [Command("debug"), RequireApplicationOwner, AllowedProcessors<TextCommandProcessor>, RequireGuild]
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
        public async ValueTask changeLogLevel(CommandContext ctx, LogLevel? logLevel = null)
        {
            if (logLevel.Equals(null))
            {
                string response = "following loglevels are available: \n```";

                foreach (var value in Enum.GetValues(typeof(LogLevel)))
                {
                    response += $"\n{(int)value} - {value}";
                }
                response += "\n```";
                response += $"\nCurrent Level: {SConfig.LogLevel}";

                await ctx.RespondAsync(response);
                return;
            }

            SConfig.LogLevel = (LogLevel)logLevel;

            SLogger.Log(LogLevel.None, $"Loglevel changed to {logLevel}", "DebugCommands.cs");
            await ctx.RespondAsync($"LogLevel changed to `{logLevel}`");
        }

        [Command("insert_wiki")]
        public async ValueTask insertWiki(CommandContext ctx, string article, int site)
        {
            await ctx.DeferResponseAsync();

            List<DiscordComponent> actionRow = new();
            actionRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"modify-wiki_article={article};site={site};", "Press me!"));

            var msg = new DiscordMessageBuilder()
                .WithContent("`DEBUG` Press the button to insert to wiki")
                .AddActionRowComponent(new DiscordActionRowComponent(actionRow));

            await ctx.EditResponseAsync(msg);

            string customId = $"modify-wiki_article={article};site={site};";
        }

        [Command("createProfiles")]
        public async ValueTask createProfiles(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var members = ctx.Guild!.Members;

            foreach (var member in members)
            {
                if (member.Value.IsBot) continue;
                SDiscordUser user = new(member.Value);
                await user.CreateProfileAsync();
            }
            await ctx.EditResponseAsync("Profile erstellt!");
        }

        [Command("currentTest")]
        public async ValueTask currentTest(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            await ctx.EditResponseAsync("Test123");
            throw new Exception("Test");
        }

        [Command("monitoringlist")]
        public async ValueTask monitoringList(CommandContext ctx)
        {
            string response = "All monitored channels:";
            var list = STwitchClient.Monitoring.ChannelsToMonitor;
            foreach (string channel in list)
            {
                response += $"\n* {channel}";
            }
            await ctx.RespondAsync(response);
        }
    }
}