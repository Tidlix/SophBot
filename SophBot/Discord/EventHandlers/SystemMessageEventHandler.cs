using DSharpPlus;
using DSharpPlus.Entities;
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
                new DiscordTextDisplayComponent($"### Ein neues Mitglied!"),
                new DiscordTextDisplayComponent($"{e.Member.Mention} hat soeben den Server betreten!"),
                new DiscordThumbnailComponent(e.Member.AvatarUrl)
            };
            await channel.SendMessageAsync(new DiscordMessageBuilder().AddContainerComponent(new (components, false, DiscordColor.Blue)));
        }

        public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
        {
            DiscordChannel channel = await e.Guild.GetSystemChannelAsync() ?? e.Guild.Channels[0];

            DiscordComponent[] components =
            {
                new DiscordTextDisplayComponent($"### Ein Verräter!"),
                new DiscordTextDisplayComponent( $"{e.Member.DisplayName} hat soeben den Server verlassen!"),
                new DiscordThumbnailComponent(e.Member.AvatarUrl)
            };
            await channel.SendMessageAsync(new DiscordMessageBuilder().AddContainerComponent(new (components, false, DiscordColor.Blue)));
        }
    }
}