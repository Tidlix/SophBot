using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.logs;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace SophBot.bot.discord.events
{
    public class TwitchEvents
    {
        public static async Task StreamOnline(object? sender, OnStreamOnlineArgs e)
        {
            SLogger.Log(LogLevel.Debug, $"Stream {e.Channel} went online", "TwitchEvents.cs");
            List<SDBValue> conditions = new();
            conditions.Add(new(SDBColumn.Name, e.Channel));
            string twitchChannel = e.Channel;
            var time = e.Stream.StartedAt.AddHours(2); // 2 For German time

            SLogger.Log(LogLevel.Debug, $"Selecting discord channels", "TwitchEvents.cs");
            List<string> discordChannels = (await SDBEngine.SelectAsync(SDBTable.TwitchMonitorings, SDBColumn.NotificationChannelID, conditions))!; 

            foreach (var discordChannelIdStr in discordChannels)
            {
                ulong.TryParse(discordChannelIdStr, out ulong discordChannelId);
                DiscordChannel discordChannel = await SBotClient.Client.GetChannelAsync(discordChannelId);
                SLogger.Log(LogLevel.Debug, $"Found channel {discordChannel}", "TwitchEvents.cs");

                conditions.Add(new(SDBColumn.NotificationChannelID, discordChannelIdStr));
                ulong mentionRoleId;
                SLogger.Log(LogLevel.Debug, $"Selection mention role", "TwitchEvents.cs");
                ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.TwitchMonitorings, SDBColumn.MentionRoleID, conditions, limit: 1))!.First(), out mentionRoleId);
                conditions.Remove(new(SDBColumn.NotificationChannelID, discordChannelIdStr));
                DiscordRole mentionRole = await discordChannel.Guild.GetRoleAsync(mentionRoleId);
                SLogger.Log(LogLevel.Debug, $"Found role {mentionRole}", "TwitchEvents.cs");

                string url = e.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080");


                DiscordComponent[] components = [
                    new DiscordTextDisplayComponent($"# {e.Stream.UserName} ist nun Live!"),
                        new DiscordSeparatorComponent(true),
                        new DiscordTextDisplayComponent($"## {e.Stream.Title}"),
                        new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(url, "test", false)),
                        new DiscordSeparatorComponent(true),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"**{mentionRole.Mention}** \n*{time.ToString("dd.MM.yyyy - HH:mm")}*"), new DiscordLinkButtonComponent($"https://twitch.tv/{twitchChannel}", label: "Jetzt auf Twitch.tv ansehen!"))
                ];

                var msg = new DiscordMessageBuilder()
                    .EnableV2Components()
                    .AddContainerComponent(new DiscordContainerComponent(components, color: DiscordColor.Purple))
                    .WithAllowedMention(new RoleMention(mentionRole));
                    SLogger.Log(LogLevel.Debug, $"Sending message", "TwitchEvents.cs");
                await discordChannel.SendMessageAsync(msg);
            }
        }
    }
}