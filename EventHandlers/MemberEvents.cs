using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Database;

namespace SophBot.EventHandlers {
    public class MemberEvents :
    IEventHandler<GuildMemberAddedEventArgs>,
    IEventHandler<GuildMemberRemovedEventArgs>,
    IEventHandler<GuildBanAddedEventArgs>
        {
        #region Member Joined
        public async Task HandleEventAsync(DiscordClient s, GuildMemberAddedEventArgs e)
        {
            ulong welcomeID = await TidlixDB.ServerConfig.readValueAsync("welcomechannel", e.Guild.Id);
            ulong ruleID = await TidlixDB.ServerConfig.readValueAsync("rulechannel", e.Guild.Id);

            DiscordChannel welcomeChannel = await e.Guild.GetChannelAsync(welcomeID);
            DiscordChannel ruleChannel = await e.Guild.GetChannelAsync(ruleID);

            DiscordComponent[] components = [
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"## Ein neues Mitglied - {e.Member.Mention}\n\n**Herzlich Willkommen auf diesem Server!**"), new DiscordThumbnailComponent(new DiscordUnfurledMediaItem(e.Member.AvatarUrl))),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent("Bitte akzeptiere die Regeln um den vollen Zugriff auf diesen Server zu erhalten!"), new DiscordLinkButtonComponent($"https://discord.com/channels/{e.Guild.Id}/{ruleChannel.Id}", label: "Zu den Regeln"))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.MidnightBlue))
            .WithAllowedMention(new UserMention(e.Member));

            await welcomeChannel.SendMessageAsync(msg);

            await TidlixDB.UserProfiles.createAsync(e.Guild.Id, e.Member.Id, 100);
        }
        #endregion

        #region Member Left
        public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
        {
            ulong welcomeID = await TidlixDB.ServerConfig.readValueAsync("welcomechannel", e.Guild.Id);

            DiscordChannel welcomeChannel = await e.Guild.GetChannelAsync(welcomeID);

            DiscordComponent[] components = [
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"## Ein Verräter - {e.Member.DisplayName}\n\n**{e.Member.DisplayName} hat soeben diesen Server verlassen!**"), new DiscordThumbnailComponent(new DiscordUnfurledMediaItem(e.Member.AvatarUrl)))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Red));

            await welcomeChannel.SendMessageAsync(msg);
        }

        #endregion

        #region Member Banned
        public async Task HandleEventAsync(DiscordClient s, GuildBanAddedEventArgs e)
        {
            var ban = await e.Guild.GetBanAsync(e.Member);
            await TidlixDB.Warnings.createAsnyc(e.Guild.Id, e.Member.Id, ban.Reason + " (Ausgeführter Ban)");
        }
        #endregion
    }
}