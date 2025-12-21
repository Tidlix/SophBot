using System.Data;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using SophBot.Discord;
using SophBot.Twitch;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;
using TwitchLib.Client;
using TwitchSharp.Entities;

namespace SophBot.Universal
{
    public class Profile
    {
        public long ID { get; private set; }
        public DiscordUser? DiscordUser { get; private set; }
        public TwitchUser? TwitchUser { get; private set; }
        public int DiscordMessages { get; private set; }
        public int TwitchMessages { get; private set; }
        public long Channelpoints { get; private set; }

        #region Constructors
        /*
        * GET PROFILE BY DISCORD ACCOUNT
        */
        public Profile(ulong discordId)
        {
            var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Profiles, ["id", "discord-id", "twitch-id", "discord-messages", "twitch-messages", "channel-points"], [new ("discord-id", "=", discordId)]);
            
            if (data.Rows.Count == 0)
            {
                // If no entry is found, a new Entry(profile) will be created!
                DatabaseEngine.InsertData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object> { {"discord-id", discordId}});
                data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Profiles, ["id", "discord-id", "twitch-id", "discord-messages", "twitch-messages", "channel-points"], [new ("discord-id", "=", discordId)]);
            } 
            ConvertData(data);

        }
        /*
        * GET PROFILE BY TWITCH ACCOUNT
        */
        public Profile(string twitchId)
        {
            var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Profiles, ["id", "discord-id", "twitch-id", "discord-messages", "twitch-messages", "channel-points"], [new ("twitch-id", "=", twitchId)]);
            if (data.Rows.Count == 0)
            {
                // If no entry is found, a new Entry(profile) will be created!
                DatabaseEngine.InsertData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object> { {"twitch-id", twitchId}});
                data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Profiles, ["id", "discord-id", "twitch-id", "discord-messages", "twitch-messages", "channel-points"], [new ("twitch-id", "=", twitchId)]);
            } 
            ConvertData(data);
        }
        /*
        * GET PROFILE BY ID
        */
        public Profile(long id)
        {
            var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Profiles, ["id", "discord-id", "twitch-id", "discord-messages", "twitch-messages", "channel-points"], [new ("id", "=", id)]);            
            if (data.Rows.Count == 0)
            {
                throw new Exception($"Profile with id \"{id}\" was not found!");
            } 
            ConvertData(data);
        }
        private void ConvertData(DataTable data)
        {
            DataRow row = data.Rows[0];
            if (data.Rows.Count > 1)
            {
                // If multiple rows are found, the one with the smallest userid will be chosen
                foreach(DataRow current in data.Rows)
                {
                    if ((long)current["id"] < (long)row["id"]) row = current;
                }
            }

            ID = (long)row["id"];
            DiscordUser = (row["discord-id"] == DBNull.Value) ? null : DiscordEngine.Client.GetUserAsync((ulong)(long)row["discord-id"]).Result;
            TwitchUser = (row["twitch-id"] == DBNull.Value) ? null : TwitchEngine.TwitchSharpClient.GetUserByIDAsync((string)row["twitch-id"]).Result;
            DiscordMessages = (int)row["discord-messages"];
            TwitchMessages = (int)row["twitch-messages"];
            Channelpoints = (long)row["channel-points"];
        }
        #endregion

        #region Functions 
        public Profile AddDiscordMessage()
        {
            DiscordMessages++;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{{"discord-messages", DiscordMessages}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile AddTwitchMessage()
        {
            TwitchMessages++;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{{"twitch-messages", TwitchMessages}}, [new ("id", "=", ID)]);
            return this;
        }

        public Profile AddChannelpoints(long channelPoints)
        {
            Channelpoints += channelPoints;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{{"channel-points", Channelpoints}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile RemoveChannelpoints(long channelPoints)
        {
            Channelpoints -= channelPoints;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{{"channel-points", Channelpoints}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile SetChannelpoints(long channelPoints)
        {
            Channelpoints = channelPoints;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{{"channel-points", Channelpoints}}, [new ("id", "=", ID)]);
            return this;
        }
        
        public Profile SyncDiscordAccount(Profile discord)
        {
            if (DiscordUser != null) throw new Exception("Discord Account is allready synced!");
            DiscordUser = discord.DiscordUser;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{
                {"discord-id", DiscordUser!.Id},
                {"discord-messages", discord.DiscordMessages},
                {"channel-points", this.Channelpoints + discord.Channelpoints}
                }, [new ("id", "=", ID)]);
            DatabaseEngine.DeleteData(DatabaseEngine.DBTable.Profiles, [new ("id", "=", discord.ID)]);
            return this;
        }
        public Profile SyncTwitchAccount(Profile twitch)
        {
            if (TwitchUser != null) throw new Exception("Twitch Account is allready synced!");
            TwitchUser = twitch.TwitchUser;
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Profiles, new Dictionary<string, object>{
                {"twitch-id", TwitchUser!.ID},
                {"twitch-messages", twitch.TwitchMessages},
                {"channel-points", this.Channelpoints + twitch.Channelpoints}
                }, [new ("id", "=", ID)]);
            DatabaseEngine.DeleteData(DatabaseEngine.DBTable.Profiles, [new ("id", "=", twitch.ID)]);
            return this;
        }

        public override string ToString()
        {
            string discordUserStr = DiscordUser is null ? "Discord not Synced!" : $"Name: {DiscordUser.GlobalName}, ID: {DiscordUser.Id}, Gesendete Nachrichten: {DiscordMessages}";
            string twitchUserStr = TwitchUser is null ? "Twitch not Synced!" : $"Name: {TwitchUser.DisplayName}, ID: {TwitchUser.ID}, Gesendete Nachrichten: {TwitchMessages}";
            return $@"UserID: {ID},
Discord: {{{discordUserStr}}}; 
Twitch: {{{twitchUserStr}}}; 
Channelpoints: {Channelpoints};";
        }
        #endregion
    }
}