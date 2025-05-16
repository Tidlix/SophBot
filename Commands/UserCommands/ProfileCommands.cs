using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.UserCommands
{
    public class ProfileCommands
    {
        [Command("Profile"), Description("Gib dein Profil aus")]
        public async ValueTask profile(CommandContext ctx)
        {
#pragma warning disable CS8602
            await ctx.DeferResponseAsync();

            var components = new DiscordComponent[] {
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"# ---------- {ctx.User.Mention}`s Profil ----------"), new DiscordThumbnailComponent(ctx.User.AvatarUrl)),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"**Name**: \n{ctx.User.GlobalName}"),
                new DiscordTextDisplayComponent($"**Mitglied seit**: \n{ctx.Member.JoinedAt.ToString("dd.MM.yyyy - HH:mm")}"),
                new DiscordTextDisplayComponent($"**Punkte**: \n{await TidlixDB.UserProfiles.getPointsAsync(ctx.Guild.Id, ctx.User.Id)}"),
                new DiscordTextDisplayComponent($"**Leaderboard Platzierung**: \n{await TidlixDB.UserProfiles.getLeaderBoardScoreAsync(ctx.Guild.Id, ctx.User.Id)} von {await TidlixDB.UserProfiles.getLeaderBoardCountAsync(ctx.Guild.Id)}")
            };

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.VeryDarkGray)));
        }


        [Command("Points")]
        public class PointCommands
        {
            [Command("Gift"), Description("Verschenke deine Punkte")]
            public static async ValueTask giftPoints(CommandContext ctx, DiscordMember target, int points)
            {
                await ctx.DeferResponseAsync();

                long availablePoints = await TidlixDB.UserProfiles.getPointsAsync(ctx.Guild.Id, ctx.User.Id);
                if (points > availablePoints)
                {
                    await ctx.EditResponseAsync("Du hast nicht genug Punkte, um diese Aktion auszuführen!");
                    return;
                }

                long targetPoints = await TidlixDB.UserProfiles.getPointsAsync(ctx.Guild.Id, target.Id);

                await TidlixDB.UserProfiles.modifyValueAsnyc("points", (availablePoints - points).ToString(), ctx.Guild.Id, ctx.User.Id);
                await TidlixDB.UserProfiles.modifyValueAsnyc("points", (targetPoints + points).ToString(), ctx.Guild.Id, target.Id);

                await ctx.EditResponseAsync(new DiscordMessageBuilder().WithContent($"Du hast {points} Punkte an {target.Mention} gegeben!").WithAllowedMention(new UserMention(target)));
            }
            [Command("Leaderboard"), Description("Gib die Top 10 des Leaderboards aus")]
            public async ValueTask leaderboard(CommandContext ctx)
            {
            #pragma warning disable CS8602
                await ctx.DeferResponseAsync();

                var components = new DiscordComponent[14];
                components[0] = new DiscordTextDisplayComponent($"# ---------- Punkte Leaderboard ----------");
                components[1] = new DiscordSeparatorComponent(true);
                int i = 1;
                foreach (var current in await TidlixDB.UserProfiles.getLeaderBoardTopAsync(ctx.Guild.Id, 10))
                {
                    string output = $"**Platz {i}: {(await ctx.Guild.GetMemberAsync(current.Key)).DisplayName}**\n{current.Value} Punkte";
                    components[++i] = new DiscordTextDisplayComponent(output);
                }
                components[12] = new DiscordSeparatorComponent(true);
                components[13] = new DiscordTextDisplayComponent($"Deine Platzierung: {await TidlixDB.UserProfiles.getLeaderBoardScoreAsync(ctx.Guild.Id, ctx.User.Id)} / Gesamte Einträge: {await TidlixDB.UserProfiles.getLeaderBoardCountAsync(ctx.Guild.Id)}");

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Gold)));
            }
        }

        [Command("Gamble"), Description("Gamble deine Punkte")]
        public class Gamble
        {
            [Command("Coinflip"), Description("Kopf oder Zahl? (Gewinnchance 50% - Auszahlung x2)")]
            public static async ValueTask coinflip(CommandContext ctx, int points)
            {
                await ctx.DeferResponseAsync();

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Coinflip"));
                components.Add(new DiscordSeparatorComponent(true));

                long availablePoints = await TidlixDB.UserProfiles.getPointsAsync(ctx.Guild.Id, ctx.User.Id);
                if (points > availablePoints)
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast nicht genug Punkte, um diese Aktion auszuführen!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }

                if (new Random().Next(10) < 5)
                {
                    await TidlixDB.UserProfiles.modifyValueAsnyc("points", (availablePoints + points).ToString(), ctx.Guild.Id, ctx.User.Id);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} gewettet und Gewonnen!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {availablePoints + points}**"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Gold)));
                }
                else
                {
                    await TidlixDB.UserProfiles.modifyValueAsnyc("points", (availablePoints - points).ToString(), ctx.Guild.Id, ctx.User.Id);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} gewettet und Verloren!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {availablePoints - points}**"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Goldenrod)));
                }
            }
            [Command("Roulette"), Description("Wähle eine Zahl 1-100 (Gewinnchance 1% Auszahlung x100)")]
            public static async ValueTask coinflip(CommandContext ctx, [Description("Auf welche Zahl willst du Wetten?")] int bet, [Description("Wie viele Punkte willst du Wetten?")] int points)
            {
                await ctx.DeferResponseAsync();

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Coinflip"));
                components.Add(new DiscordSeparatorComponent(true));

                if (bet < 1 ||  bet > 100)
                {
                    components.Add(new DiscordTextDisplayComponent("Deine Wette ist außerhalb des gültigen Wertebereichs (1-100)!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }

                long availablePoints = await TidlixDB.UserProfiles.getPointsAsync(ctx.Guild.Id, ctx.User.Id);
                if (points > availablePoints)
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast nicht genug Punkte, um diese Aktion auszuführen!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }

                int x = new Random().Next(100);
                if (x == bet)
                {
                    await TidlixDB.UserProfiles.modifyValueAsnyc("points", (availablePoints + (points * 100)).ToString(), ctx.Guild.Id, ctx.User.Id);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} auf {bet} gewettet und Gewonnen!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {availablePoints + (points * 100)}**"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Gold)));
                }
                else
                {
                    await TidlixDB.UserProfiles.modifyValueAsnyc("points", (availablePoints - points).ToString(), ctx.Guild.Id, ctx.User.Id);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} auf {bet} gewettet, aber die Zahl wurde {x}!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {availablePoints - points }"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Goldenrod)));
                }
            }
        }
    }
}