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

            string twitchChannel = e.Channel;
            var time = e.Stream.StartedAt.AddHours(2); // 2 For German time

            SLogger.Log(LogLevel.Debug, $"Selecting discord channels", "TwitchEvents.cs");
            var notifications = await SDBEngine.SelectFromAsync(
                table: "twitchmonitorings",
                columns: ["notificationchannel", "mentionrole"],
                conditions: new Dictionary<string, object> { { "channel", twitchChannel } });

            foreach (var notification in notifications)
            {
                DiscordChannel notificationChannel = await SBotClient.Client.GetChannelAsync((ulong)notification[0]);
                DiscordRole mentionRole = await notificationChannel.Guild.GetRoleAsync((ulong)notification[1]);

                SLogger.Log(LogLevel.Debug, $"Found channel {notificationChannel}", "STwichClient.cs");
                SLogger.Log(LogLevel.Debug, $"Found guild ({mentionRole})", "STwichClient");

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
                await notificationChannel.SendMessageAsync(msg);
            }
        }
    }
}