using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Objects;

namespace SophBot.Commands.UserCommands
{
    public class ProfileCommands
    {
        [Command("Profile"), Description("Gib dein Profil aus")]
        public async ValueTask profile(CommandContext ctx, DiscordMember? member = null)
        {
#pragma warning disable CS8602, CS8604, CS8625
            await ctx.DeferResponseAsync();

            if (member == null) member = ctx.Member;

            var components = new DiscordComponent[] {
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"# ---------- {member.Mention}`s Profil ----------"), new DiscordThumbnailComponent(member.AvatarUrl)),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"**Name**: \n{member.DisplayName}"),
                new DiscordTextDisplayComponent($"**Mitglied seit**: \n{member.JoinedAt.ToString("dd.MM.yyyy - HH:mm")}"),
                new DiscordTextDisplayComponent($"**Punkte**: \n{await TDatabase.UserProfiles.getPointsAsync(ctx.Guild.Id, member.Id)}"),
                new DiscordTextDisplayComponent($"**Leaderboard Platzierung**: \n{await TDatabase.UserProfiles.getLeaderBoardScoreAsync(ctx.Guild.Id, member.Id)} von {await TDatabase.UserProfiles.getLeaderBoardCountAsync(ctx.Guild.Id)}")
            };

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.VeryDarkGray)));
        }


        [Command("Points")]
        public class PointCommands
        {
            [Command("Gift"), Description("Verschenke deine Punkte")]
            public static async ValueTask giftPoints(CommandContext ctx, DiscordMember target, ulong points)
            {
                await ctx.DeferResponseAsync();

                TDiscordMember member = new(ctx.Member);
                TDiscordMember targetMember = new(target);

                if (await member.getGuildPointsAsnyc() < points)
                {
                    await ctx.EditResponseAsync("Du hast nicht genug Punkte um diese Aktion auszuführen");
                    return;
                }
                await member.removeGuildPointsAsnyc(points);
                await targetMember.addGuildPointsAsnyc(points);

                await ctx.EditResponseAsync(new DiscordMessageBuilder().WithContent($"Du hast {points} Punkte an {target.Mention} gegeben!").WithAllowedMention(new UserMention(target)));
            }

            [Command("Leaderboard"), Description("Gib die Top 10 des Leaderboards aus")]
            public async ValueTask leaderboard(CommandContext ctx)
            {
            #pragma warning disable CS8602
                await ctx.DeferResponseAsync();
                TDiscordMember member = new(ctx.Member);

                var components = new List<DiscordComponent>();
                components.Add(new DiscordTextDisplayComponent($"# ---------- Punkte Leaderboard ----------"));
                components.Add(new DiscordSeparatorComponent(true));
                int i = 1;
                foreach (var current in await TDatabase.UserProfiles.getLeaderBoardTopAsync(ctx.Guild.Id, 10))
                {
                    string output = $"**Platz {i}: {(await ctx.Guild.GetMemberAsync(current.Key)).DisplayName}**\n{current.Value} Punkte";
                    components.Add(new DiscordTextDisplayComponent(output));
                }
                components.Add(new DiscordSeparatorComponent(true));
                components.Add(new DiscordTextDisplayComponent($"Deine Platzierung: {await member.getGuildPointsAsnyc()} / Gesamte Einträge: {await TDatabase.UserProfiles.getLeaderBoardCountAsync(ctx.Guild.Id)}"));

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Gold)));
            }
        }

        [Command("Gamble"), Description("Gamble deine Punkte")]
        public class Gamble
        {
            [Command("Coinflip"), Description("Kopf oder Zahl? (Gewinnchance 50% - Auszahlung x2)")]
            public static async ValueTask coinflip(CommandContext ctx, ulong points)
            {
                await ctx.DeferResponseAsync();
                TDiscordMember member = new(ctx.Member);

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Coinflip"));
                components.Add(new DiscordSeparatorComponent(true));

                if (points > await member.getGuildPointsAsnyc())
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast nicht genug Punkte, um diese Aktion auszuführen!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }
                else if (points <= 0)
                {
                    components.Add(new DiscordTextDisplayComponent("Du musst mindestens 1 Punkt wetten, um diese Aktion auszuführen!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }

                if (new Random().Next(10) < 5)
                {
                    await member.addGuildPointsAsnyc(points);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} gewettet und Gewonnen!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {await member.getGuildPointsAsnyc()}"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Gold)));
                }
                else
                {
                    await member.removeGuildPointsAsnyc(points);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} gewettet und Verloren!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {await member.getGuildPointsAsnyc()}"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Goldenrod)));
                }
            }
            [Command("Roulette"), Description("Wähle eine Zahl 1-100 (Gewinnchance 1% Auszahlung x100)")]
            public static async ValueTask roulette(CommandContext ctx, [Description("Auf welche Zahl willst du Wetten?")] int bet, [Description("Wie viele Punkte willst du Wetten?")] ulong points)
            {
                await ctx.DeferResponseAsync();
                TDiscordMember member = new(ctx.Member);

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Roulette"));
                components.Add(new DiscordSeparatorComponent(true));

                if (bet < 1 ||  bet > 100)
                {
                    components.Add(new DiscordTextDisplayComponent("Deine Wette ist außerhalb des gültigen Wertebereichs (1-100)!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }

                if (points > await member.getGuildPointsAsnyc())
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast nicht genug Punkte, um diese Aktion auszuführen!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }
                else if (points <= 0)
                {
                    components.Add(new DiscordTextDisplayComponent("Du musst mindestens 1 Punkt wetten, um diese Aktion auszuführen!"));
                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red)));
                    return;
                }

                int x = new Random().Next(100);
                if (x == bet)
                {
                    await member.addGuildPointsAsnyc((ulong)(bet * 100));
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} auf {bet} gewettet und Gewonnen!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {await member.getGuildPointsAsnyc()}"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Gold)));
                }
                else
                {
                    await member.removeGuildPointsAsnyc(points);
                    components.Add(new DiscordTextDisplayComponent($"**{ctx.User.Mention} hat {points} auf {bet} gewettet, aber die Zahl wurde {x}!**"));
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordTextDisplayComponent($"Deine Punkte: {await member.getGuildPointsAsnyc()}"));

                    await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Goldenrod)));
                }
            }
        }
    }
}