using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SophBot.bot.discord.events
{
    public class LogEvents :
        IEventHandler<MessageDeletedEventArgs>,
        IEventHandler<MessageUpdatedEventArgs>,
        IEventHandler<GuildBanAddedEventArgs>,
        IEventHandler<GuildMemberUpdatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageDeletedEventArgs e)
        {
            if (e.Message.Author!.IsCurrent) return;
            await LogMessageAsync(e.Guild, $"Eine Nachricht von {e.Message.Author.Mention} wurde gelöscht!", $"**Author:**\n{e.Message.Author}\n\n**Nachricht gesendet am:**\n{e.Message.Timestamp.ToString("dd.MM.yy - HH:mm:ss")}\n\n**Message ID:**\n{e.Message.Id}\n\n**Die Nachricht:**\n{e.Message.Content}", e.Message.JumpLink.AbsoluteUri);//$"https://discord.com/channels/{e.Guild.Id}/{e.Channel!.Id}");
        }
        public async Task HandleEventAsync(DiscordClient s, MessageUpdatedEventArgs e)
        {
            if (e.Message.Author!.IsBot || e.Message.Author.IsCurrent) return;

            await LogMessageAsync(e.Guild, $"{e.Message.Author!.Mention} hat eine Nachricht bearbeitet (1/2)!", $"**Author:**\n{e.Message.Author}\n\n**Ursprüngliche Nachricht gesendet am:**\n{e.Message.Timestamp.ToString("dd.MM.yy - HH:mm:ss")}\n\n**Message ID:**\n{e.Message.Id}\n\n**Alte Nachricht:**\n{e.MessageBefore!.Content}");
            await LogMessageAsync(e.Guild, $"{e.Message.Author!.Mention} hat eine Nachricht bearbeitet (2/2)!", $"**Neue Nachricht:**\n{e.Message.Content}", e.Message.JumpLink.AbsoluteUri);
        }
        public async Task HandleEventAsync(DiscordClient s, GuildBanAddedEventArgs e)
        {
            DiscordBan ban = await e.Guild.GetBanAsync(e.Member);
            await LogMessageAsync(e.Guild, $"{e.Member.Mention} wurde gebannt", $"**Name:**\n{ban.User}\n\n**Grund:**\n{ban.Reason}");
        }
        public async Task HandleEventAsync(DiscordClient s, GuildMemberUpdatedEventArgs e)
        {
            string nicknameOld = e.NicknameBefore;
            string nicknameNew = e.NicknameAfter;

            if (nicknameNew != nicknameOld)
            {
                await LogMessageAsync(e.Guild, $"{e.Member.Mention} hat seinen Nickname geändert", $"**Alter Name:**\n{nicknameOld}\n\n**Neuer Name:** {nicknameNew}");
            }
        }


        public static async ValueTask LogMessageAsync(DiscordGuild guild, string title, string content, string? jumpLink = null)
        {
            SDiscordServer server = new(guild);
            DiscordChannel logChannel = await server.getChannelAsync(SDiscordChannel.LogChannel);

            List<DiscordComponent> components = new();
            components.Add(new DiscordTextDisplayComponent("## " + title));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent(content));
            if (jumpLink != null) components.Add(new DiscordActionRowComponent(new[] { new DiscordLinkButtonComponent(jumpLink, "Zur Nachricht") }));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"-# {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}"));

            await logChannel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components)));
        }
    }
}