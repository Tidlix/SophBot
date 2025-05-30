using DSharpPlus.Entities;
using SophBot.Configuration;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace SophBot.Objects
{
    public static class TTwitchClient
    {
        #pragma warning disable CS8618
        public static LiveStreamMonitorService Monitoring;

            public static async ValueTask CreateTwitchMonitoring()
            {
                try
                {
                    #pragma warning disable CS8629
                    TwitchAPI api = new TwitchAPI();
                    api.Settings.ClientId = Config.Twitch.ClientId;
                    api.Settings.Secret = Config.Twitch.ClientSecret;
                    api.Settings.AccessToken = Config.Twitch.AccessToken;

                    Monitoring = new LiveStreamMonitorService(api, 60);
                    //Monitoring.OnStreamOnline += async (s, e) => await StreamOnline(s, e);

                    var list = new List<string>();
                    var channelList = await TDatabase.TwitchMonitorings.getAllMonitoringsAsync();
                    foreach (var channel in channelList)
                    {
                        list.Add(channel);
                    }

                    Monitoring.SetChannelsByName(list);
                    Monitoring.Start();
                }
                catch (Exception e)
                {
                    TLog.sendLog(e.Message, TLog.MessageType.Warning);
                }
            }

            private static async Task StreamOnline(object? sender, OnStreamOnlineArgs e)
            {
                string twitchChannel = e.Channel;
                var time = e.Stream.StartedAt;

                List<ulong> discordChannels = await TDatabase.TwitchMonitorings.getMonitoringChannelsAsync(twitchChannel.ToLower());

                foreach (ulong discordChannelId in discordChannels)
                {
                    DiscordChannel discordChannel = await TBotClient.Client.GetChannelAsync(discordChannelId);
                    DiscordGuild guild = await TBotClient.Client.GetGuildAsync((ulong)discordChannel.GuildId);

                    ulong mentionRoleId = await TDatabase.ServerConfig.readValueAsync("mentionrole", guild.Id);
                    DiscordRole mentionRole = await guild.GetRoleAsync(mentionRoleId);

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

                    await discordChannel.SendMessageAsync(msg);
                }
            }
    }
}