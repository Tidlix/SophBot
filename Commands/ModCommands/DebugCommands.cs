using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.Objects;

namespace SophBot.Commands.ModCommands {
    [Command("Debug"), RequireApplicationOwner, RequirePermissions(DiscordPermission.Administrator)] 
    public class DebugCommands {
        [Command("Test"), Description("Test if the bot can respond to commands")]
        public static async ValueTask Ping (CommandContext ctx) {
            await ctx.RespondAsync($"Test Complete! The Bot can respond!" );
        }

        [Command("currentTest"), Description("Test Command")]
        public static async ValueTask currentTestCmd (CommandContext ctx) {
            await ctx.DeferResponseAsync();
            await ctx.DeleteResponseAsync();
            
        }
        [Command("Loglevel"), Description("Changes the Log-Level of the Bot")]
        public static async ValueTask logLevelCmd(CommandContext ctx, LogLevel level)
        {
            await ctx.RespondAsync("Log Level changing... \n*this may take a few minutes*");

            if (TBotClient.Client != null) TLog.sendLog("Client not null");

            await TBotClient.Client!.DisconnectAsync();
            await TBotClient.CreateDiscordClient(level);

            TLog.sendLog($"Log-Level changed to {level.ToString()} by {ctx.User.GlobalName}", TLog.MessageType.Message);
        }

        [Command("serverconfig")] 
        public class debugConfig {
            [Command("create")]
            public static async ValueTask createConfig (CommandContext ctx, ulong serverid, ulong welcomeChannel, ulong ruleChannel, ulong memberRole, ulong mentionRole) {
                try {
                    await TDatabase.ServerConfig.createAsnyc(serverid, ruleChannel, welcomeChannel, memberRole, mentionRole);
                    await ctx.RespondAsync("Config created!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't create server config - " + e.Message);
                }
            }
            [Command("delete")]
            public static async ValueTask deleteConfig (CommandContext ctx, ulong serverid) {
                try {
                    await TDatabase.ServerConfig.deleteAsync(serverid);
                    await ctx.RespondAsync("Config deleted!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't delete server config - " + e.Message);
                }
            }
        }

        [Command("Userprofiles")]
        public class userProfiles {
            [Command("create")]
            public static async ValueTask createConfig (CommandContext ctx, DiscordMember user) {
                #pragma warning disable CS8602
                try {
                    TDiscordMember member = new(user);
                    await member.createProfileAsync();
                    await ctx.RespondAsync("Profile created!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't create user profile - " + e.Message);
                }
            }
            [Command("guildCreate")]
            public static async ValueTask deleteConfig (CommandContext ctx, int points) {
                try {
                    var users = ctx.Guild.GetAllMembersAsync();

                    await foreach(var user in users) {
                        try {
                            if (user.IsBot) continue;
                            
                            await TDatabase.UserProfiles.createAsync(ctx.Guild.Id, user.Id, points);
                        } catch (Exception e) { TLog.sendLog(e.Message, TLog.MessageType.Warning); }
                        
                    }

                    await ctx.RespondAsync("Profiles created!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't create user profiles - " + e.Message);
                }
            }
        }
    }
}