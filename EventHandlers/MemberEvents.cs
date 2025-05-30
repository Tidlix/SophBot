using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Objects;

namespace SophBot.EventHandlers {
    public class MemberEvents :
    IEventHandler<GuildMemberAddedEventArgs>,
    IEventHandler<GuildMemberRemovedEventArgs>,
    IEventHandler<GuildBanAddedEventArgs>
        {
        #region Member Joined
        public async Task HandleEventAsync(DiscordClient s, GuildMemberAddedEventArgs e)
        {
            TDiscordGuild guild = new(e.Guild);
            TDiscordMember member = new(e.Member);

            DiscordChannel welcomeChannel = await guild.getChannelAsnyc(TDiscordGuild.Channel.Welcome);
            DiscordChannel ruleChannel = await guild.getChannelAsnyc(TDiscordGuild.Channel.Rules);

            DiscordComponent[] components = [
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"## Ein neues Mitglied - {e.Member.Mention}\n\n**Herzlich Willkommen auf diesem Server!**"), new DiscordThumbnailComponent(new DiscordUnfurledMediaItem(e.Member.AvatarUrl))),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent("Bitte akzeptiere die Regeln um den vollen Zugriff auf diesen Server zu erhalten!"), new DiscordLinkButtonComponent($"https://discord.com/channels/{e.Guild.Id}/{ruleChannel.Id}", label: "Zu den Regeln"))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.MidnightBlue))
            .WithAllowedMention(new UserMention(e.Member));

            await welcomeChannel.SendMessageAsync(msg);

            await member.createProfileAsync();
        }
        #endregion

        #region Member Left
        public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
        {
            TDiscordGuild guild = new(e.Guild);
            DiscordChannel welcomeChannel = await guild.getChannelAsnyc(TDiscordGuild.Channel.Welcome);

            DiscordComponent[] components = [
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"## Ein Verräter - {e.Member.DisplayName}\n\n**{e.Member.DisplayName} hat soeben diesen Server verlassen!**"), new DiscordThumbnailComponent(new DiscordUnfurledMediaItem(e.Member.AvatarUrl)))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red));

            await welcomeChannel.SendMessageAsync(msg);
        }

        #endregion

        #region Member Banned
        public async Task HandleEventAsync(DiscordClient s, GuildBanAddedEventArgs e)
        {
            TDiscordMember member = new(e.Member);
            var ban = await e.Guild.GetBanAsync(e.Member);
            await member.addWarningAsnyc(ban.Reason);
        }
        #endregion
    }
}