using DSharpPlus.Entities;
using TwitchSharp.Entitys;

namespace SophBot.Universal
{
    public class Profile
    {
        public ulong ID { get; private set; }
        public DiscordUser? DiscordUser { get; private set; }
        public TwitchUser? TwitchUser { get; private set; }
        public int DiscordMessages { get; private set; }
        public int TwitchMessages { get; private set; }
        public ulong Channelpoints { get; private set; }

        public Profile(DiscordUser discordUser)
        {
            DiscordUser = discordUser;
            // Get remaining from db table profiles
        }
        public Profile(TwitchUser twitchUser)
        {
            TwitchUser = twitchUser;
            // Get remaining from db table profiles
        }
        public Profile(ulong id)
        {
            ID = id;
            // Get remaining from db table profiles
        }

#pragma warning disable CS0114
        public string ToString()
        {
            string discordUserStr = DiscordUser is null ? "Discord not Synced!" : $"Name: {DiscordUser.GlobalName}, ID: {DiscordUser.Id}, Sended Messages: {DiscordMessages}";
            string twitchUserStr = TwitchUser is null ? "Twitch not Synced!" : $"Name: {TwitchUser.DisplayName}, ID: {TwitchUser.ID}, Sended Messages: {TwitchMessages}";
            return $"UserID: {ID}; Discord Account: {discordUserStr}; Twitch Account: {twitchUserStr}; Channelpoints: {Channelpoints};";
        }
#pragma warning restore CS0114
    }
}