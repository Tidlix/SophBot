using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Database;

namespace SophBot.EventHandlers {
    public class MemberEvents : 
    IEventHandler<GuildMemberAddedEventArgs>, 
    IEventHandler<GuildMemberRemovedEventArgs>, 
    IEventHandler<GuildBanAddedEventArgs>
    {
        #region Member Joined
        public async Task HandleEventAsync(DiscordClient s, GuildMemberAddedEventArgs e)
        {
            ulong welcomeID = await TidlixDB.readServerconfig("welcomechannel", e.Guild.Id);
            ulong ruleID = await TidlixDB.readServerconfig("rulechannel", e.Guild.Id);

            DiscordChannel welcomeChannel = await e.Guild.GetChannelAsync(welcomeID);
            DiscordChannel ruleChannel = await e.Guild.GetChannelAsync(ruleID);

            var embed = new DiscordEmbedBuilder() {
                Title = "Ein neues Mitglied!",
                Description = $"{e.Member.DisplayName} hat soeben den Server betreten!",
                Color = DiscordColor.Blue,
                Footer = new () {
                    Text = $"Bitte akzeptiere die Regeln in #{ruleChannel.Name} um Zugriff auf den ganzen Server zu erhalten!"
                },
                Thumbnail = new () {
                    Url = e.Member.AvatarUrl
                }
            };

            await welcomeChannel.SendMessageAsync(embed);
        }
        #endregion

        #region Member Left
        public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
        {
            ulong welcomeID = await TidlixDB.readServerconfig("welcomechannel", e.Guild.Id);

            DiscordChannel welcomeChannel = await e.Guild.GetChannelAsync(welcomeID);

            var embed = new DiscordEmbedBuilder() {
                Title = "Ein Veräter!",
                Description = $"{e.Member.DisplayName} hat soeben den Server verlassen!",
                Color = DiscordColor.Red,
                Thumbnail = new () {
                    Url = e.Member.AvatarUrl
                }
            };

            await welcomeChannel.SendMessageAsync(embed);
        }

        #endregion

        #region Member Banned
        public async Task HandleEventAsync(DiscordClient s, GuildBanAddedEventArgs e)
        {
            var ban = await e.Guild.GetBanAsync(e.Member);
            await TidlixDB.createWarning(e.Guild.Id, e.Member.Id, ban.Reason + " (Ausgeführter Ban)");
        }
        #endregion
    }
}