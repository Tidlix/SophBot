using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands
{
    public class PointCommands
    {
        [Command("Points")]
        public class Points
        {
            [Command("Leaderboard"), Description("Gib das aktuelle Leaderboard (Channelpoints) aus")]
            public async ValueTask pointLB(CommandContext ctx)
            {
                await LeaderBoardCmdAsync(ctx, "points");
            }
            [Command("Give"), Description("Gib einem Nutzer deine Channelpoints ab")]
            public async ValueTask pointGive(CommandContext ctx, DiscordMember target, ulong points)
            {
                await ctx.DeferResponseAsync();
                SDiscordUser[] users = new SDiscordUser[]
                {
                    new(ctx.Member!),
                    new(target)
                };

                if (await users[0].GetPointsAsync() < points)
                {
                    await ctx.EditResponseAsync("Du hast nicht genug Channelpoints für diese Aktion!");
                    return;
                }
                await users[0].RemovePointsAsync(points);
                await users[1].AddPointsAsync(points);

                await ctx.EditResponseAsync($"Du hast {points} Channelpoints an {target.Mention} gegeben!");
            }
            /*
            [Command("Set"), Description("Lege die Channelpoints eines Nutzers fest"), RequirePermissions(DiscordPermission.Administrator)]
            public async ValueTask pointSet(CommandContext ctx, DiscordMember target, ulong points)
            {
                await ctx.DeferResponseAsync();
                SDiscordUser user = new(target);

                await user.SetPointsAsync(points);

                await ctx.EditResponseAsync($"Du hast die Channelpoints von {target.Mention} auf {points} Channelpoints gesetzt!");
            }
            */
        }

        [Command("Leaderboard")]
        public class Leaderboard
        {
            [Command("Points"), Description("Gib das aktuelle Leaderboard (Channelpoints) aus")]
            public async ValueTask pointLB(CommandContext ctx)
            {
                await LeaderBoardCmdAsync(ctx, "points");
            }
            [Command("messages"), Description("Gib das aktuelle Leaderboard (Nachrichten) aus")]
            public async ValueTask messageLB(CommandContext ctx)
            {
                await LeaderBoardCmdAsync(ctx, "messages");
            }
        }



        private static async ValueTask LeaderBoardCmdAsync(CommandContext ctx, string type)
            {
                await ctx.DeferResponseAsync();

                SDiscordServer server = new(ctx.Guild!);
                SDiscordUser user = new(ctx.Member!);
                var dict = (type == "points") ? await server.getPointsLeaderboardAsync() : await server.getMessagesLeaderboardAsync(); 

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent((type == "points") ? "## Channelpoints Bestenliste:" : "## Messages Bestenliste:"));
                components.Add(new DiscordSeparatorComponent(true));
                int i = 0;
                string leaderboard = "";
                foreach (var current in dict)
                {
                    i++;
                    leaderboard += $"**{i} - {current.Key}: ** {current.Value} {((type == "points") ? "Channelpoints" : "gesendete Nachrichten")}\n";
                }
                components.Add(new DiscordTextDisplayComponent(leaderboard));
                components.Add(new DiscordSeparatorComponent(true));
                components.Add(new DiscordTextDisplayComponent($"Deine {((type == "points") ? "Channelpoints:" : "gesendeten Nachrichten:")} {((type == "points") ? await user.GetPointsAsync() : await user.GetMessageCountAsnyc())}"));

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Gold)));
            }
    }
}