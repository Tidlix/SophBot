using SophBot.Commands.ContextChecks;
using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using SophBot.Database;

namespace SophBot.Commands.ModCommands {
    [Command("Debug"), RequireBotOwner, RequirePermissions(DiscordPermission.Administrator)] 
    public class DebugCommands {
        [Command("Test"), Description("Test if the bot can respond to commands"), RequireApplicationOwner]
        public static async ValueTask Ping (CommandContext ctx) {
            await ctx.RespondAsync($"Test Complete! The Bot can respond!" );
        }


        [Command("serverconfig")] 
        public class debugConfig {
            [Command("create")]
            public static async ValueTask createConfig (CommandContext ctx, ulong serverid, ulong welcomeChannel, ulong ruleChannel, ulong memberRole) {
                try {
                    await TidlixDB.createServerconfig(serverid, ruleChannel, welcomeChannel, memberRole);
                    await ctx.RespondAsync("Config created!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't create server config - " + e.Message);
                }
            }
            [Command("delete")]
            public static async ValueTask deleteConfig (CommandContext ctx, ulong serverid) {
                try {
                    await TidlixDB.deleteServerconfig(serverid);
                    await ctx.RespondAsync("Config deleted!");
                } catch (Exception e) {
                    await ctx.RespondAsync("Couldn't delete server config - " + e.Message);
                }
            }
        }
    }
}