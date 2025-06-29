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
            SLogger.Log(LogLevel.Debug, $"Got Twitch add command.", "TwitchCommands.cs");
            List<SDBValue> values = new();
            values.Add(new(SDBColumn.Name, ChannelName));
            values.Add(new(SDBColumn.NotificationChannelID, ctx.Channel.Id.ToString()));
            values.Add(new(SDBColumn.MentionRoleID, MentionRole.Id.ToString()));

            SLogger.Log(LogLevel.Debug, $"Adding channel.", "TwitchCommands.cs");
            await ctx.RespondAsync("Überwachung zu diesem Channel hinzugefügt");
            await SDBEngine.InsertAsync(values, SDBTable.TwitchMonitorings);
            SLogger.Log(LogLevel.Debug, $"Channel Added.", "TwitchCommands.cs");

            SLogger.Log(LogLevel.Debug, $"Updating monitoring list.", "TwitchCommands.cs");
            List<string> list = (await SDBEngine.SelectAsync(SDBTable.TwitchMonitorings, SDBColumn.Name))!;
            list = list.GroupBy(s => s).Select(g => g.Key).ToList();
            STwitchClient.Monitoring.SetChannelsByName(list);
            SLogger.Log(LogLevel.Debug, $"Monitoring list updated.", "TwitchCommands.cs");
        }

        [Command("Remove"), Description("Lösche eine Kanal-Überwachung von diesem Channel")] 
        public async Task removeMonitoring (CommandContext ctx, string ChannelName) {
            SLogger.Log(LogLevel.Debug, $"Got Twitch remove command.", "TwitchCommands.cs");
            List<SDBValue> conditions = new();
            conditions.Add(new(SDBColumn.Name, ChannelName));
            conditions.Add(new(SDBColumn.NotificationChannelID, ctx.Channel.Id.ToString()));

            SLogger.Log(LogLevel.Debug, $"Removing channel.", "TwitchCommands.cs");
            await ctx.RespondAsync("Überwachungen dieses Channels wurde entfernt!");
            await SDBEngine.DeleteAsync(SDBTable.TwitchMonitorings, conditions);
            SLogger.Log(LogLevel.Debug, $"Channel removed.", "TwitchCommands.cs");

            SLogger.Log(LogLevel.Debug, $"Updating monitoring list.", "TwitchCommands.cs");
            var list = await SDBEngine.SelectAsync(SDBTable.TwitchMonitorings, SDBColumn.Name);
            STwitchClient.Monitoring.SetChannelsByName(list);
            SLogger.Log(LogLevel.Debug, $"Monitoring list updated.", "TwitchCommands.cs");
        }
    }
}