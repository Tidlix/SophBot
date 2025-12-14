using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace SophBot.Discord.DebugCommands
{
    [Command("get"), RequireApplicationOwner, RequirePermissions(DiscordPermission.Administrator)]
    public class DebugGetCommands
    {
        [Command("channels")]
        public async Task debugGetChannels(TextCommandContext ctx)
        {
            DiscordGuild g = ctx.Guild!;
            DiscordChannel? defaultChannel = g.GetDefaultChannel();
            DiscordChannel? afkChannel = await g.GetAfkChannelAsync();
            DiscordChannel? rulesChannel = await g.GetRulesChannelAsync();
            DiscordChannel? systemChannel = await g.GetSystemChannelAsync();
            DiscordChannel? widgetChannel = await g.GetWidgetChannelAsync();
            DiscordChannel? safetyAlertsChannel = await g.GetSafetyAlertsChannelAsync();
            DiscordChannel? publicUpdatesChannel = await g.GetPublicUpdatesChannelAsync();

            string channels = @$"List of current Channels:
DEFAULT: {(defaultChannel is null ? "null" : defaultChannel.Mention)}
AFK: {(afkChannel is null ? "null" : afkChannel.Mention)}
RULES: {(rulesChannel is null ? "null" : rulesChannel.Mention)}
SYSTEM: {(systemChannel is null ? "null" : systemChannel.Mention)}
WIDGET: {(widgetChannel is null ? "null" : widgetChannel.Mention)}
SAFETY_ALERTS: {(safetyAlertsChannel is null ? "null" : safetyAlertsChannel.Mention)}
PUBLIC_UPDATES: {(publicUpdatesChannel is null ? "null" : publicUpdatesChannel.Mention)}";

            await ctx.Channel.SendMessageAsync(channels);
        }
    }
}