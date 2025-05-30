using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Objects;

namespace SophBot.EventHandlers {
    public class BotEvents : 
    IEventHandler<GuildDownloadCompletedEventArgs>, 
    IEventHandler<GuildCreatedEventArgs>, 
    IEventHandler<GuildDeletedEventArgs>,
    IEventHandler<ClientErrorEventArgs>
    {

        #region Bot Ready
        public Task HandleEventAsync(DiscordClient s, GuildDownloadCompletedEventArgs e)
        {
            TLog.sendLog("Bot started and ready!", TLog.MessageType.Message);
            return Task.CompletedTask;
        }
        #endregion

        #region Bot Added
        public async Task HandleEventAsync(DiscordClient s, GuildCreatedEventArgs e)
        {
            try
            {
                TDiscordGuild guild = new(e.Guild);
                await guild.createDefaultConfigAsnyc();

                DiscordChannel welcomeChannel = await guild.getChannelAsnyc(TDiscordGuild.Channel.Welcome);
            }
            catch (Exception ex)
            {
                try
                {
                    await e.Guild.GetDefaultChannel()!.SendMessageAsync(ex.Message);
                }
                catch
                {
                    TLog.sendLog("Couldn't send server config error message! - Leaving Server...");
                    await e.Guild.LeaveAsync();
                }
            }
        }
        #endregion

        #region Bot Removed
        public async Task HandleEventAsync(DiscordClient s, GuildDeletedEventArgs e)
        {
            TDiscordGuild guild = new(e.Guild);
            await guild.deleteConfigAsnyc();
        }
        #endregion

        #region Error
        public Task HandleEventAsync(DiscordClient s, ClientErrorEventArgs e)
        {
            TLog.sendLog($"{e.EventName} - {e.Exception.Message}", TLog.MessageType.Error);
            return Task.CompletedTask;
        }
        #endregion
    }
}