using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.EventHandlers {
    public class BotEvents : 
    IEventHandler<GuildDownloadCompletedEventArgs>, 
    IEventHandler<GuildCreatedEventArgs>, 
    IEventHandler<GuildDeletedEventArgs>
    {

        #region Bot Ready
        public async Task HandleEventAsync(DiscordClient s, GuildDownloadCompletedEventArgs e)
        {
            DiscordActivity activity = new DiscordActivity("Sophia`s Stream", DiscordActivityType.Watching);
            await s.UpdateStatusAsync(activity);
            await MessageSystem.sendMessage("Bot started and ready!", MessageType.Message());
        }
        #endregion

        #region Bot Added
        public async Task HandleEventAsync(DiscordClient s, GuildCreatedEventArgs e)
        {
            #pragma warning disable CS8600, CS8602

            DiscordGuild g = e.Guild;


            DiscordChannel ruleChannel = await g.GetRulesChannelAsync();
            DiscordChannel welcomeChannel = await g.GetRulesChannelAsync();
            DiscordRole memberRole = await getMemberRole(g);
            

            ruleChannel = ruleChannel ?? g.GetDefaultChannel();
            welcomeChannel = welcomeChannel ?? g.GetDefaultChannel();

            
            await MessageSystem.sendMessage($"Bot was added to {g.Name}!", MessageType.Message());
            try {
                await TidlixDB.createServerconfig(g.Id, ruleChannel.Id, welcomeChannel.Id, memberRole.Id);
                await ruleChannel.SendMessageAsync("Dieser Channel wurde automatisch als Regel-Kanal eingerichtet. Diese Einstellung kannst du mit /modifyconfig ändern!");
                await welcomeChannel.SendMessageAsync("Dieser Channel wurde automatisch als Wilkommens-Kanal eingerichtet. Diese Einstellung kannst du mit /modifyconfig ändern!");
                await welcomeChannel.SendMessageAsync($"{memberRole.Mention} wurde automatisch als Member-Rolle eingerichtet. Diese Einstellung kannst du mit /modifyconfig ändern");
            } catch (Exception ex) {
                await g.GetDefaultChannel().SendMessageAsync("Fehler - Server Konfiguration konnte nicht erstellt werden! Bitte kontaktiere den Entwickler dieses Bots!");
                await MessageSystem.sendMessage($"Serverconfig for server {g.Name}({g.Id}) couldn't be generated! - {ex.Message}", MessageType.Error());
            }
        }
        private static async Task<DiscordRole> getMemberRole(DiscordGuild g) {
            #pragma warning disable CS8603
            var roles = await g.GetRolesAsync();
            foreach(DiscordRole role in roles) {
                if (role.CheckPermission(DiscordPermission.Administrator) == 0) return role;
            }
            return roles.FirstOrDefault();
        }
        #endregion

        #region Bot Removed
        public async Task HandleEventAsync(DiscordClient s, GuildDeletedEventArgs e)
        {
            await MessageSystem.sendMessage($"Bot was added to {e.Guild.Name}!", MessageType.Message());
            try {
                await TidlixDB.deleteServerconfig(e.Guild.Id);
            } catch (Exception ex) {
                await MessageSystem.sendMessage($"Serverconfig for server {e.Guild.Name}({e.Guild.Id}) couldn't be deleted! - {ex.Message}", MessageType.Error());
            }
        }
        #endregion
    }
}