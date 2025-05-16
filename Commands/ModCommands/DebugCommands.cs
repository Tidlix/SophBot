using SophBot.Commands.ContextChecks;
using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.ModCommands {
    [Command("Debug"), RequireBotOwner, RequirePermissions(DiscordPermission.Administrator)] 
    public class DebugCommands {
        [Command("Test"), Description("Test if the bot can respond to commands"), RequireApplicationOwner]
        public static async ValueTask Ping (CommandContext ctx) {
            await ctx.RespondAsync($"Test Complete! The Bot can respond!" );
        }

        [Command("currentTest"), Description("Test Command"), RequireApplicationOwner]
        public static async ValueTask currentTestCmd (CommandContext ctx) {
            await ctx.DeferResponseAsync();
            await ctx.DeleteResponseAsync();
            
        }

        [Command("serverconfig")] 
        public class debugConfig {
            [Command("create")]
            public static async ValueTask createConfig (CommandContext ctx, ulong serverid, ulong welcomeChannel, ulong ruleChannel, ulong memberRole, ulong mentionRole) {
                try {
                    await TidlixDB.ServerConfig.createAsnyc(serverid, ruleChannel, welcomeChannel, memberRole, mentionRole);
                    await ctx.RespondAsync("Config created!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't create server config - " + e.Message);
                }
            }
            [Command("delete")]
            public static async ValueTask deleteConfig (CommandContext ctx, ulong serverid) {
                try {
                    await TidlixDB.ServerConfig.deleteAsync(serverid);
                    await ctx.RespondAsync("Config deleted!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't delete server config - " + e.Message);
                }
            }
        }

        [Command("Userprofiles")]
        public class userProfiles {
            [Command("create")]
            public static async ValueTask createConfig (CommandContext ctx, DiscordUser user, int points) {
                #pragma warning disable CS8602
                try {
                    await TidlixDB.UserProfiles.createAsync(ctx.Guild.Id, user.Id, points);
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
                            
                            await TidlixDB.UserProfiles.createAsync(ctx.Guild.Id, user.Id, points);
                        } catch (Exception e) { await Log.sendMessage(e.Message, MessageType.Warning()); }
                        
                    }

                    await ctx.RespondAsync("Profiles created!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't create user profiles - " + e.Message);
                }
            }
        }
    }
}