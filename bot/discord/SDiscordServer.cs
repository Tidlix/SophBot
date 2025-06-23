using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.discord.features;
using SophBot.bot.logs;

namespace SophBot.bot.discord
{
    public enum SDiscordChannel {
        WelcomeChannel, 
        RuleChannel
    }
    public enum SDiscordRole {
        MemberRole
    }

    public class SDiscordServer
    {
        public DiscordGuild Guild;
        public CCEngine Commands;

        public SDiscordServer(DiscordGuild guild)
        {
            this.Guild = guild;
            this.Commands = new(guild);
        }

        public async ValueTask createConfigAsync(DiscordChannel welcomeChannel, DiscordChannel ruleChannel, DiscordRole memberRole)
        {
            List<SDBValue> values = new();
            values.Add(new SDBValue(SDBColumn.ServerID, Guild.Id.ToString()));
            values.Add(new SDBValue(SDBColumn.WelcomeChannelID, welcomeChannel.Id.ToString()));
            values.Add(new SDBValue(SDBColumn.RuleChannelID, ruleChannel.Id.ToString()));
            values.Add(new SDBValue(SDBColumn.MemberRoleID, memberRole.Id.ToString()));

            await SDBEngine.InsertAsync(values, SDBTable.ServerConfig);
        }
        public async ValueTask modifyConfigAsync(DiscordChannel? welcomeChannel = null, DiscordChannel? ruleChannel = null, DiscordRole? memberRole = null)
        {
            List<SDBValue> values = new();
            if (welcomeChannel! != null!) values.Add(new SDBValue(SDBColumn.WelcomeChannelID, welcomeChannel.Id.ToString()));
            if (ruleChannel! != null!) values.Add(new SDBValue(SDBColumn.RuleChannelID, ruleChannel.Id.ToString()));
            if (memberRole! != null!) values.Add(new SDBValue(SDBColumn.MemberRoleID, memberRole.Id.ToString()));

            if (values.Count == 0) throw new Exception("Modifying values are all null!");

            List<SDBValue> condition = new();
            condition.Add(new SDBValue(SDBColumn.ServerID, Guild.Id.ToString()));

            await SDBEngine.ModifyAsync(values, SDBTable.ServerConfig, condition);
        }

        public async ValueTask<DiscordChannel> getChannelAsync(SDiscordChannel type)
        {
            try
            {
                ulong channelID;
                switch (type)
                {
                    case SDiscordChannel.WelcomeChannel:
                        ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.ServerConfig, SDBColumn.WelcomeChannelID))!.First(), out channelID);
                        break;
                    case SDiscordChannel.RuleChannel:
                        ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.ServerConfig, SDBColumn.RuleChannelID))!.First(), out channelID);
                        break;

                    default:
                        throw new Exception("Unknown channel");
                }

                return await Guild.GetChannelAsync(channelID);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Couldn't get Server Channel", "SDiscordServer.cs", ex);
                throw new Exception(ex.Message);
            }
        }
        public async ValueTask<DiscordRole> getRoleAsync(SDiscordRole role)
        {
            try
            {
                ulong roleID;
                switch (role)
                {
                    case SDiscordRole.MemberRole:
                        ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.ServerConfig, SDBColumn.MemberRoleID))!.First(), out roleID);
                        break;


                    default:
                        throw new Exception("Unknown role");
                }

                return await Guild.GetRoleAsync(roleID);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Couldn't get Server Channel", "SDiscordServer.cs", ex);
                throw new Exception(ex.Message);
            }
        }


        public async ValueTask<Dictionary<string, ulong>> getPointsLeaderboardAsync()
        {
            Dictionary<string, ulong> result = new();

            List<SDBValue> conditions = new();
            conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
            var users = await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.UserID, conditions, SDBColumn.Points, true, 10);

            foreach (var user in users!)
            {
                ulong.TryParse(user, out ulong userId);

                string name;
                ulong points;

                try
                {
                    DiscordUser current = await Guild.GetMemberAsync(userId);
                    name = current.GlobalName;
                }
                catch
                {
                    name = "404 - User not found!";
                }

                SLogger.Log(LogLevel.Debug, $"Getting Points for {name} (Leaderboard)", "SDiscordServer.cs");
                List<SDBValue> conditions2 = new();
                conditions2.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                conditions2.Add(new SDBValue(SDBColumn.UserID, user));
                ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.Points, conditions2))!.First(), out points);

                result.Add(name, points);
            }

            return result;
        }
        public async ValueTask<Dictionary<string, ulong>> getMessagesLeaderboardAsync()
        {
            Dictionary<string, ulong> result = new();

            List<SDBValue> conditions = new();
            conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
            var users = await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.UserID, conditions, SDBColumn.Number, true, 10);

            foreach (var user in users!)
            {
                ulong.TryParse(user, out ulong userId);

                string name;
                ulong points;

                try
                {
                    DiscordUser current = await Guild.GetMemberAsync(userId);
                    name = current.GlobalName;
                }
                catch
                {
                    name = "404 - User not found!";
                }

                SLogger.Log(LogLevel.Debug, $"Getting Points for {name} (Leaderboard)", "SDiscordServer.cs");
                List<SDBValue> conditions2 = new();
                conditions2.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                conditions2.Add(new SDBValue(SDBColumn.UserID, user));
                ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.Number, conditions2))!.First(), out points);

                result.Add(name, points);
            }

            return result;
        }
    }
}