using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SophBot.bot.discord.events
{
    public class WelcomeEvents :
        IEventHandler<GuildMemberAddedEventArgs>,
        IEventHandler<GuildMemberRemovedEventArgs>,
        IEventHandler<GuildBanAddedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, GuildMemberAddedEventArgs e)
        {
            SDiscordServer server = new(e.Guild);

            DiscordChannel welcomeChannel = await server.getChannelAsync(SDiscordChannelType.WelcomeChannel);
            DiscordChannel ruleChannel = await server.getChannelAsync(SDiscordChannelType.RuleChannel);

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
        }

        public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
        {
            SDiscordServer server = new(e.Guild);
            DiscordChannel welcomeChannel = await server.getChannelAsync(SDiscordChannelType.WelcomeChannel);

            DiscordComponent[] components = [
                new DiscordSectionComponent(
                    new DiscordTextDisplayComponent($"## Ein Verräter - {e.Member.DisplayName}\n\n**{e.Member.DisplayName} hat soeben diesen Server verlassen!**"),
                    new DiscordThumbnailComponent(new DiscordUnfurledMediaItem(e.Member.AvatarUrl)))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red));

            await welcomeChannel.SendMessageAsync(msg);
        }

        public async Task HandleEventAsync(DiscordClient s, GuildBanAddedEventArgs e)
        {
            SDiscordServer server = new(e.Guild);
            DiscordChannel welcomeChannel = await server.getChannelAsync(SDiscordChannelType.WelcomeChannel);
            DiscordBan ban = await e.Guild.GetBanAsync(e.Member);

            DiscordComponent[] components = [
                new DiscordSectionComponent(
                    new DiscordTextDisplayComponent($"## Banhammer - {e.Member.DisplayName}\n\n**Grund: {ban.Reason}**"),
                    new DiscordThumbnailComponent(new DiscordUnfurledMediaItem(e.Member.AvatarUrl)))
            ];

            var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Red));

            await welcomeChannel.SendMessageAsync(msg);
        }
    }
}