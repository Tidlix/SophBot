using System.Data;
using DSharpPlus.Entities;
using SophBot.Discord;
using SophBot.Universal;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchSharp.Entities;

namespace SophBot.Twitch.Events
{
    public static class StreamOnlineEvents
    {
        public static async Task OnStreamOnlineAsync(object? sender, OnStreamOnlineArgs args)
        {
            string id = args.Stream.UserId;

            var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Monitorings, ["discord-channel", "mention-role"], [new ("twitch-channel", "=", id)]);

            foreach(DataRow current in data.Rows)
            {
                DiscordChannel channel = await DiscordEngine.Client.GetChannelAsync((ulong)(long)current["discord-channel"]);
                DiscordRole role = await channel.Guild.GetRoleAsync((ulong)(long)current["mention-role"]);
                DiscordComponent[] components = [
                    new DiscordTextDisplayComponent($"# {args.Stream.UserName} ist nun Live!"),
                        new DiscordSeparatorComponent(true),
                        new DiscordTextDisplayComponent($"## {args.Stream.Title}"),
                        new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(args.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080"), "Stream Thumbnail", false)),
                        new DiscordSeparatorComponent(true),
                        new DiscordSectionComponent(
                            new DiscordTextDisplayComponent($"**{role.Mention}** \n*{args.Stream.StartedAt.ToString("dd.MM.yyyy - HH:mm")}*"), 
                            new DiscordLinkButtonComponent($"https://twitch.tv/{args.Channel.ToLower()}", label: "Jetzt auf Twitch.tv ansehen!"))
                ];
                await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components, false, DiscordColor.Purple)).WithAllowedMention(new RoleMention(role)));
            }
            TwitchUser streamer = await TwitchEngine.TwitchSharpClient.GetUserByIDAsync(id);
            string aiMessage = await GeminiEngine.GenerateResponseAsync(new SystemAiRequest($"{streamer.DisplayName} hat gerade einen Livestream gestartet. Kündige deine Anwesenheit in einer kurzen Nachricht an."));
            await streamer.SendChatMessageAsync(aiMessage);
        }
    }
}