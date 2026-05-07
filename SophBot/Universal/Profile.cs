using System.Data;
using System.Threading.Channels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Hosting;
using SophBot.Discord;
using TwitchSharp.Api.Clients;

namespace SophBot.Universal
{
    public class Profile
    {
        public long ID { get; private set; }
        public DiscordUser? DiscordUser { get; private set; }
        public UserData? TwitchUser { get; private set; }
        public long DiscordMessages { get; private set; }
        public long TwitchMessages { get; private set; }
        public long Channelpoints { get; private set; }
        public string[]? AiNotes { get; private set; }

        private string[] columnList = ["id", "discord-id", "twitch-id", "discord-messages", "twitch-messages", "channel-points", "ai-notes"];


        #region Constructors
        /*
        * GET PROFILE BY DISCORD ACCOUNT
        */
        public Profile(ulong discordId)
        {
            var data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Profiles, columnList, [new ("discord-id", "=", discordId)]);
            
            if (data.Rows.Count == 0)
            {
                // If no entry is found, a new Entry(profile) will be created!
                Program.GetService<DatabaseService>().InsertData(DBTable.Profiles, new Dictionary<string, object> { {"discord-id", discordId}});
                data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Profiles, columnList, [new ("discord-id", "=", discordId)]);
            } 
            ConvertData(data);
        }
        /*
        * GET PROFILE BY TWITCH ACCOUNT
        */
        public Profile(string twitchId)
        {
            var data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Profiles, columnList, [new ("twitch-id", "=", twitchId)]);
            if (data.Rows.Count == 0)
            {
                // If no entry is found, a new Entry(profile) will be created!
                Program.GetService<DatabaseService>().InsertData(DBTable.Profiles, new Dictionary<string, object> { {"twitch-id", twitchId}});
                data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Profiles, columnList, [new ("twitch-id", "=", twitchId)]);
            } 
            ConvertData(data);
        }
        /*
        * GET PROFILE BY ID
        */
        public Profile(long id)
        {
            var data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Profiles, columnList, [new ("id", "=", id)]);            
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
            DiscordUser = (row["discord-id"] == DBNull.Value) ? null :  Program.GetService<DiscordClient>().GetUserAsync((ulong)(long)row["discord-id"]).Result;
            //TwitchUser = (row["twitch-id"] == DBNull.Value) ? null : TwitchEngine.MainClient.Users.GetUsersAsync(ids: [(string)row["twitch-id"]]).Result[0];
            DiscordMessages = (long)row["discord-messages"];
            TwitchMessages = (long)row["twitch-messages"];
            Channelpoints = (long)row["channel-points"];
            AiNotes = (row["ai-notes"] == DBNull.Value) ? null : (string[]?)row["ai-notes"];
        }
        #endregion

        #region Functions 
        public Profile AddDiscordMessage()
        {
            DiscordMessages++;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"discord-messages", DiscordMessages}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile AddTwitchMessage()
        {
            TwitchMessages++;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"twitch-messages", TwitchMessages}}, [new ("id", "=", ID)]);
            return this;
        }

        public Profile AddChannelpoints(long channelPoints)
        {
            Channelpoints += channelPoints;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"channel-points", Channelpoints}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile RemoveChannelpoints(long channelPoints)
        {
            Channelpoints -= channelPoints;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"channel-points", Channelpoints}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile SetChannelpoints(long channelPoints)
        {
            Channelpoints = channelPoints;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"channel-points", Channelpoints}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile AddAiNote(string value)
        {
            if (AiNotes is null)
                AiNotes = [value];
            else
                AiNotes = AiNotes.Append(value).ToArray();
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"ai-notes", AiNotes}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile SetAiNote(int index, string value)
        {
            if (AiNotes is null || AiNotes.Length < index)
                throw new Exception("Index was not found in array");
            AiNotes[index] = value;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{{"ai-notes", AiNotes}}, [new ("id", "=", ID)]);
            return this;
        }
        public Profile SyncDiscordAccount(Profile discord)
        {
            if (DiscordUser != null) throw new Exception("Discord Account is allready synced!");
            DiscordUser = discord.DiscordUser;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{
                {"discord-id", DiscordUser!.Id},
                {"discord-messages", discord.DiscordMessages},
                {"channel-points", this.Channelpoints + discord.Channelpoints}
                }, [new ("id", "=", ID)]);
            Program.GetService<DatabaseService>().DeleteData(DBTable.Profiles, [new ("id", "=", discord.ID)]);
            return this;
        }
        public Profile SyncTwitchAccount(Profile twitch)
        {
            if (TwitchUser != null) throw new Exception("Twitch Account is allready synced!");
            TwitchUser = twitch.TwitchUser;
            Program.GetService<DatabaseService>().ModifyData(DBTable.Profiles, new Dictionary<string, object>{
                {"twitch-id", TwitchUser!.Id},
                {"twitch-messages", twitch.TwitchMessages},
                {"channel-points", this.Channelpoints + twitch.Channelpoints}
                }, [new ("id", "=", ID)]);
            Program.GetService<DatabaseService>().DeleteData(DBTable.Profiles, [new ("id", "=", twitch.ID)]);
            return this;
        }

        public override string ToString()
        {
            string result = string.Empty;
            result += $"**Nutzer ID:** {ID}\n";
            if (DiscordUser is not null)
            {
                result += $"**Discord:**\n  **Name:** {DiscordUser.GlobalName}\n  **Discord ID:** {DiscordUser.Id}\n";
            }
            if (TwitchUser is not null)
            {
                result += $"**Twitch:**\n  **Name:** {TwitchUser.DisplayName}\n  **Twitch ID:** {TwitchUser.Id}\n";
            }
            result += $"**Gesendete Nachrichten:**\n  **Discord:** {DiscordMessages}\n  **Twitch:** {TwitchMessages}\n  **Gesamt:** {DiscordMessages+TwitchMessages}\n";
            result += $"**Channelpoints:** {Channelpoints}\n";

            return result;
        }
        public string AsAiString()
        {
            string result = $@"{{
userId = {ID},
discordId = {(DiscordUser is null ? "null" : DiscordUser.Id)},
twitchId = {(TwitchUser is null ? "null" : TwitchUser.Id)},
discordUsername = {(DiscordUser is null ? "null" : DiscordUser.GlobalName)},
twitchUsername = {(TwitchUser is null ? "null" : TwitchUser.DisplayName)},
discordMessages = {DiscordMessages},
twitchMessages = {TwitchMessages},
channelpoints = {Channelpoints},
aiNotes = [{(AiNotes is null ? "null" : $"{{{string.Join(';', AiNotes)}}}")}]
}}";
            return result;
        }
        #endregion
    }
}