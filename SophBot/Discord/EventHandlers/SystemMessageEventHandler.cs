using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;

namespace SophBot.Discord.EventHandlers
{
    public class SystemMessageEventHandler :
        IEventHandler<GuildMemberAddedEventArgs>,
        IEventHandler<GuildMemberRemovedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, GuildMemberAddedEventArgs e)
        {
            DiscordChannel channel = await e.Guild.GetSystemChannelAsync() ?? e.Guild.Channels[0];

            DiscordComponent[] components =
            {
                new DiscordTextDisplayComponent($"## Ein neues Mitglied!"),
                new DiscordSectionComponent(
                    new DiscordTextDisplayComponent($"### {e.Member.Mention} hat soeben den Server betreten!"),
                    new DiscordThumbnailComponent(e.Member.AvatarUrl)
                )
            };
            await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, false, DiscordColor.Blue)));
        }

        public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
        {
            DiscordChannel channel = await e.Guild.GetSystemChannelAsync() ?? e.Guild.Channels[0];
            DiscordBan? ban;
            try 
            {
                ban = await e.Guild.GetBanAsync(e.Member);
            } catch
            {
                ban = null;
            }

            string leaveMessage = $"### {e.Member.DisplayName} hat soeben den Server verlassen!";
            if (ban is not null) leaveMessage = $"### {e.Member.DisplayName} wurde soeben vom Server gebannt! \n**Grund:** {ban.Reason}";

            DiscordComponent[] components =
            {
                new DiscordTextDisplayComponent($"## Ein Verräter!"),
                new DiscordSectionComponent(
                    new DiscordTextDisplayComponent(leaveMessage),
                    new DiscordThumbnailComponent(e.Member.AvatarUrl)
                )
            };
            await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Red)));
        }
    }
}