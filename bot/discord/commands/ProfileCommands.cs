using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands
{
    public class ProfileCommands
    {
        [Command("Profile")]
        public async ValueTask profile(CommandContext ctx, DiscordMember? member = null)
        {
            await ctx.DeferResponseAsync();
            member ??= ctx.Member!;
            SDiscordUser user = new(member);

            List<DiscordComponent> components = new();
            components.Add(new DiscordSectionComponent(
                new DiscordTextDisplayComponent($"# {member.Mention}`s Profil:"),
                new DiscordThumbnailComponent(member.AvatarUrl)
            ));
            components.Add(new DiscordTextDisplayComponent($"**Name:** {member.DisplayName}"));
            components.Add(new DiscordTextDisplayComponent($"**Mitglied seit:** {member.JoinedAt.ToString("dd.MM.yyyy - HH:mm")}"));
            components.Add(new DiscordTextDisplayComponent($"**Channelpoints:** {await user.GetPointsAsync()}"));
            components.Add(new DiscordTextDisplayComponent($"**Gesendete Nachrichten:** -- Coming Soon --"));

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Gold)));
        }

        [Command("Leaderboard")]
        public class Leaderboard
        {
            [Command("Points")]
            public async ValueTask pointLB(CommandContext ctx)
            {
                await ctx.DeferResponseAsync();

                SDiscordServer server = new(ctx.Guild!);
                SDiscordUser user = new(ctx.Member!);
                var dict = await server.getPointsLeaderboardAsync();

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Channelpoints Bestenliste:"));
                components.Add(new DiscordSeparatorComponent(true));
                int i = 0;
                string leaderboard = "";
                foreach (var current in dict)
                {
                    i++;
                    leaderboard += $"**{i} - {current.Key}: ** {current.Value} Channelpoints \n";
                }
                components.Add(new DiscordTextDisplayComponent(leaderboard));
                components.Add(new DiscordSeparatorComponent(true));
                components.Add(new DiscordTextDisplayComponent($"Deine Channelpoints: {await user.GetPointsAsync()}"));

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Gold)));
            }
            [Command("messages")]
            public async ValueTask messageLB(CommandContext ctx)
            {
                await ctx.DeferResponseAsync();

                SDiscordServer server = new(ctx.Guild!);
                SDiscordUser user = new(ctx.Member!);
                var dict = await server.getMessagesLeaderboardAsync();

                List<DiscordComponent> components = new();
                components.Add(new DiscordTextDisplayComponent("## Messages Bestenliste:"));
                components.Add(new DiscordSeparatorComponent(true));
                int i = 0;
                string leaderboard = "";
                foreach (var current in dict)
                {
                    i++;
                    leaderboard += $"**{i} - {current.Key}: ** {current.Value} gesendete Nachrichten \n";
                }
                components.Add(new DiscordTextDisplayComponent(leaderboard));
                components.Add(new DiscordSeparatorComponent(true));
                components.Add(new DiscordTextDisplayComponent($"Deine gesendeten Nachrichten: {await user.GetMessageCountAsnyc()}"));

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Gold)));
            }
        }
    }
}