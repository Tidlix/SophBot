using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using SophBot.Configuration;
using SophBot.Database;

namespace SophBot.Commands.ModCommands {
    [Command("TwitchMonitoring"), RequirePermissions(DiscordPermission.Administrator)]
    public class TwitchCommands {
        [Command("Add"), Description("Füge einen neue Kanal-Überwachung zu diesem Channel hinzu")] 
        public async Task addMonitoring (CommandContext ctx, string ChannelName) {
            await ctx.RespondAsync("Überwachung zu diesem Channel hinzugefügt");
            await TidlixDB.newMonitoring(ChannelName.ToLower(), ctx.Channel.Id);

            var list = await TidlixDB.getAllMonitorings();
            list.Add(ChannelName.ToLower());
            Services.Twitch.Monitoring.SetChannelsByName(list);
        }
        [Command("Remove"), Description("Lösche alle Kanal-Überwachung von diesem Channel")] 
        public async Task removeMonitoring (CommandContext ctx) {
            await ctx.RespondAsync("Alle Überwachungen dieses Channels entfernt!");
            await TidlixDB.removeMonitoring(ctx.Channel.Id);

            var list = await TidlixDB.getAllMonitorings();
            Services.Twitch.Monitoring.SetChannelsByName(list);
        }
    }
}