using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Twitch;
using SophBot.Universal;
using TwitchSharp.Entities;

namespace SophBot.Discord.SlashCommands
{
    public class TwitchCommands
    {
        [Command("add-monitoring")]
        public async Task addTwitchMonitoring(CommandContext ctx, [Description("Der Twitch-Kanal der Überwacht werden soll")] string twitchChannel, [Description("Die Rolle die benachrichtigt werden soll")] DiscordRole role)
        {
            await ctx.DeferResponseAsync();

            TwitchUser twitch = await TwitchEngine.TwitchSharpClient.GetUserByLoginAsync(twitchChannel.ToLower());
            DatabaseEngine.InsertData(DatabaseEngine.DBTable.Monitorings, new Dictionary<string, object>{ {"discord-channel", ctx.Channel.Id}, {"twitch-channel", twitch.ID}, {"mention-role", role.Id}});
            await ctx.EditResponseAsync($"Überwachung für https://twitch.tv/{twitchChannel.ToLower()} mit Rolle {role.Mention} hinzugefügt!");
        }
    }
}