using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using SophBot.Configuration;
using SophBot.Objects;

namespace SophBot.Commands.ModCommands {
    [Command("TwitchMonitoring"), RequirePermissions(DiscordPermission.Administrator)]
    public class TwitchCommands {
        [Command("Add"), Description("Füge einen neue Kanal-Überwachung zu diesem Channel hinzu")] 
        public async Task addMonitoring (CommandContext ctx, string ChannelName) {
            await ctx.RespondAsync("Überwachung zu diesem Channel hinzugefügt");
            await TDatabase.TwitchMonitorings.createAsnyc(ChannelName.ToLower(), ctx.Channel.Id);

            var list = await TDatabase.TwitchMonitorings.getAllMonitoringsAsync();
            list.Add(ChannelName.ToLower());
            TTwitchClient.Monitoring.SetChannelsByName(list);
        }
        [Command("Remove"), Description("Lösche alle Kanal-Überwachung von diesem Channel")] 
        public async Task removeMonitoring (CommandContext ctx) {
            await ctx.RespondAsync("Alle Überwachungen dieses Channels entfernt!");
            await TDatabase.TwitchMonitorings.deleteAsnyc(ctx.Channel.Id);

            var list = await TDatabase.TwitchMonitorings.getAllMonitoringsAsync();
            TTwitchClient.Monitoring.SetChannelsByName(list);
        }
    }
}