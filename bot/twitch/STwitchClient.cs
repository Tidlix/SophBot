using Newtonsoft.Json;
using SophBot.bot.conf;
using SophBot.bot.database;
using SophBot.bot.discord.events;
using SophBot.bot.logs;
using TwitchLib.Api;
using TwitchLib.Api.Services;


using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using SophBot.bot.discord;

namespace SophBot.bot.twitch
{
    public static class STwitchClient
    {
#pragma warning disable CS8618
        public static LiveStreamMonitorService Monitoring;

        public static async ValueTask CreateTwitchMonitoringAsnyc()
        {
            try
            {
                SLogger.Log(LogLevel.Debug, "Creating Twitch Monitoring client", "STwitchClient.cs");
                TwitchAPI api = new TwitchAPI();
                api.Settings.ClientId = SConfig.Twitch.ClientId;
                api.Settings.Secret = SConfig.Twitch.ClientSecret;
                api.Settings.AccessToken = await getAccessTokenAsync();

                SLogger.Log(LogLevel.Debug, "Create Monitor Service", "STwitchClient.cs");
                Monitoring = new LiveStreamMonitorService(api, 60);
                Monitoring.OnStreamOnline += async (s, e) => await StreamOnline(s, e);
                Monitoring.OnServiceStarted += (s, e) => SLogger.Log(LogLevel.Debug, "Service started!", "STwitchClient.cs");
                Monitoring.OnChannelsSet += (s, e) => SLogger.Log(LogLevel.Debug, $"Set channels {string.Join(", ", e.Channels)}!", "STwitchClient.cs");

                SLogger.Log(LogLevel.Debug, "Setting Monitoring list", "STwitchClient.cs");
                var list = new List<string>();
                var channelList = await SDBEngine.SelectAsync(SDBTable.TwitchMonitorings, SDBColumn.Name);
                if (channelList == null) throw new Exception("Couldn't find channels for twitch monitoring");
                foreach (string channel in channelList)
                {
                    SLogger.Log(LogLevel.Debug, $"Found channel {channel},", "STwitchClient.cs");
                    if (list.Contains(channel)) continue;
                    SLogger.Log(LogLevel.Debug, $"Added channel {channel} to list,", "STwitchClient.cs");
                    list.Add(channel);
                }

                Monitoring.SetChannelsByName(list);
                SLogger.Log(LogLevel.Debug, "Start Monitoring", "STwitchClient.cs");
                Monitoring.Start();
                SLogger.Log(LogLevel.Debug, "Monitoring started", "STwitchClient.cs");
            }
            catch (Exception e)
            {
                SLogger.Log(LogLevel.Warning, "Something went wrong while creating twitch monitoring", "STwitchClient.cs", e);
            }
        }



        private static async ValueTask<string> getAccessTokenAsync()
        {
            SLogger.Log(LogLevel.Debug, "Getting Access Token", "STwitchClient.cs");
            string destination = "https://id.twitch.tv/oauth2/token";
            HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                {"client_id", SConfig.Twitch.ClientId},
                {"client_secret", SConfig.Twitch.ClientSecret },
                {"grant_type", "client_credentials" },
                {"scope", "chat:read chat:edit user:bot" }
            };
            var request = new FormUrlEncodedContent(values);

            SLogger.Log(LogLevel.Debug, "Sending AT Request", "STwitchClient.cs");
            var response = await client.PostAsync(destination, request);
            var responseString = await response.Content.ReadAsStringAsync();
            SLogger.Log(LogLevel.Debug, "Got AT Response", "STwitchClient.cs");

            var data = JsonConvert.DeserializeObject<tokenResponse>(responseString);
            if (data != null)
            {
                SLogger.Log(LogLevel.Debug, $"Found AT {data.access_token}", "STwitchClient.cs");
                return data.access_token;
            }
            else
            {
                throw new Exception("Access Token request failed");
            }
        }
        internal class tokenResponse
        {
            public string access_token = "errorTwitch";
        }


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