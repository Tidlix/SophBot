using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SophBot.Discord.EventHandlers
{
    public class LogEventHandler :
        IEventHandler<MessageDeletedEventArgs>,
        IEventHandler<MessageUpdatedEventArgs>,
        IEventHandler<GuildBanAddedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageDeletedEventArgs e)
        {
            string delContent = e.Message.Content; 
            if (delContent.Length >= 3800)
                delContent = delContent.Substring(0, 3800) + "[...]";
            DiscordMessageBuilder msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(new DiscordContainerComponent([
                    new DiscordTextDisplayComponent("### Gelöschte Nachricht!"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent($"Gesendet von: {e.Message.Author!.Mention} \nGesendet am: {e.Message.CreationTimestamp.ToLocalTime()} \nGesendet in: {e.Message.Channel!.Mention}"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent(delContent)
                ]));
            await Program.GetService<DiscordService>().getLogChannel().SendMessageAsync(msg);
        }

        public async Task HandleEventAsync(DiscordClient s, MessageUpdatedEventArgs e)
        {
            string oldContent = e.MessageBefore!.Content; 
            string newContent = e.Message.Content;
            if (oldContent == newContent) return;

            if (oldContent.Length >= 3800)
                oldContent = oldContent.Substring(0, 1900) + "[...]";
            if (newContent.Length >= 3800)
                newContent = newContent.Substring(0, 1900) + "[...]";
            DiscordMessageBuilder msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(new DiscordContainerComponent([
                    new DiscordTextDisplayComponent("### Bearbeitete Nachricht!"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent($"Gesendet von: {e.Message.Author!.Mention} \nGesendet am: {e.Message.CreationTimestamp.ToLocalTime()} \nGesendet in: {e.Message.Channel!.Mention}"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent(oldContent),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent(newContent),
                    new DiscordActionRowComponent([new DiscordLinkButtonComponent(e.Message.JumpLink.AbsoluteUri, "Zur Nachricht")])
                ]));
            await Program.GetService<DiscordService>().getLogChannel().SendMessageAsync(msg);
        }

        public async Task HandleEventAsync(DiscordClient s, GuildBanAddedEventArgs e)
        {
            DiscordBan ban = await e.Guild.GetBanAsync(e.Member);

            DiscordMessageBuilder msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(new DiscordContainerComponent([
                    new DiscordTextDisplayComponent("### Gebannter Nutzer!"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent($"Gebannter Nutzer: {ban.User.GlobalName} ({ban.User.Id}) \nGrund: {ban.Reason}")
                ]));
            await Program.GetService<DiscordService>().getLogChannel().SendMessageAsync(msg);
        }
    }
}