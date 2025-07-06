using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands
{
    public class ProfileCommands
    {
        [Command("Profile"), Description("Lass dir (d)ein Profil anzeigen")]
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
            components.Add(new DiscordTextDisplayComponent($"**Gesendete Nachrichten:** {await user.GetMessageCountAsnyc()}"));

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Gold)));
        }
    }
}