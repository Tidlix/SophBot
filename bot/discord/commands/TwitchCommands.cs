using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.logs;
using SophBot.bot.twitch;

namespace SophBot.bot.discord.commands
{
    [Command("TwitchMonitoring"), RequirePermissions(DiscordPermission.Administrator)]
    public class TwitchCommands {
        [Command("Add"), Description("Füge einen neue Kanal-Überwachung zu diesem Channel hinzu")]
        public async Task addMonitoring(CommandContext ctx, string ChannelName, DiscordRole MentionRole)
        {
            Dictionary<string, object>[] values = [
                new Dictionary<string, object> {
                    {"channel", ChannelName.ToLower()},
                    {"notificationchannelid", ctx.Channel.Id},
                    {"mentionroleid", MentionRole.Id}
                }
            ];
            await SDBEngine.InsertToAsync("twitchmonitorings", values, false);

            var channels = await SDBEngine.SelectFromAsync("twitchmonitorings", ["channel"]);
            List<string> channelList = new();
            foreach (var channel in channels) channelList.Add((string)channel[0]);

            STwitchClient.Monitoring.SetChannelsByName(channelList);
            SLogger.Log(LogLevel.Debug, $"Monitoring list updated.", "TwitchCommands.cs");
        }

        [Command("Remove"), Description("Lösche eine Kanal-Überwachung von diesem Channel")] 
        public async Task removeMonitoring (CommandContext ctx, string ChannelName) {
            Dictionary<string, object> conditions = 
                new Dictionary<string, object> {
                    {"channel", ChannelName.ToLower()},
                    {"notificationchannelid", ctx.Channel.Id},
                };
            await SDBEngine.DeleteFromAsync("twitchmonitorings", conditions);

            var channels = await SDBEngine.SelectFromAsync("twitchmonitorings", ["channel"]); 
            List<string> channelList = new();
            foreach (var channel in channels) channelList.Add((string)channel[0]);

            STwitchClient.Monitoring.SetChannelsByName(channelList);
            SLogger.Log(LogLevel.Debug, $"Monitoring list updated.", "TwitchCommands.cs");
        }
    }
}