using DSharpPlus.Entities;

namespace SophBot.Objects
{
    public class TUserWarning
    {
        public ulong? WarnId;
        public DiscordGuild Guild;
        public DiscordUser User;
        public string Date;
        public string Reason;

        public TUserWarning(ulong? warnId, ulong guildId, ulong userId, string date, string reason)
        {
            WarnId = warnId;
            Guild = TBotClient.Client.GetGuildAsync(guildId).Result;
            User = TBotClient.Client.GetUserAsync(userId).Result;
            Date = date;
            Reason = reason;
        }

    }
}