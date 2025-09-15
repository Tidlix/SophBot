using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.discord.features;
using SophBot.bot.logs;

namespace SophBot.bot.discord
{
    public enum SDiscordChannel
    {
        WelcomeChannel,
        RuleChannel,
        LogChannel
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

        public async ValueTask createConfigAsync(DiscordChannel welcomeChannel, DiscordChannel ruleChannel, DiscordChannel logChannel, DiscordRole memberRole)
        {
            Dictionary<string, object>[] values = [
                new Dictionary<string, object> {
                    {"serverid", Guild.Id},
                    {"rulechannelid", ruleChannel.Id},
                    {"welcomechannelid", welcomeChannel.Id},
                    {"logchannelid", logChannel.Id},
                    {"memberroleid", memberRole.Id}
                }
            ];

            await SDBEngine.InsertToAsync("warnings", values, false);
        }
        public async ValueTask modifyConfigAsync(DiscordChannel? welcomeChannel = null, DiscordChannel? ruleChannel = null, DiscordChannel? logChannel = null, DiscordRole? memberRole = null)
        {

            Dictionary<string, object> values = new Dictionary<string, object>();


            if (welcomeChannel is not null) values.Add("welcomechannelid", welcomeChannel.Id);
            if (ruleChannel is not null) values.Add("rulechannelid", ruleChannel.Id);
            if (logChannel is not null) values.Add("logchannelid", logChannel.Id);
            if (memberRole is not null) values.Add("memberroleid", memberRole.Id);

            await SDBEngine.ModifyAtAsync("serverconfig", values, new Dictionary<string, object> { {"serverid", Guild.Id} } );
        }

        public async ValueTask<DiscordChannel> getChannelAsync(SDiscordChannel type)
        {
            try
            {
                string channel = string.Empty;
                switch (type)
                {
                    case SDiscordChannel.WelcomeChannel:
                        channel = "welcomechannelid";
                        break;
                    case SDiscordChannel.RuleChannel:
                        channel = "rulechannelid";
                        break;
                    case SDiscordChannel.LogChannel:
                        channel = "logchannelid";
                        break;

                    default:
                        throw new Exception("Unknown channel");
                }
                ulong channelID = (ulong)(await SDBEngine.SelectFromAsync("serverconfig", [channel], new Dictionary<string, object> { { "serverid", Guild.Id } }))[0][0];

                return await Guild.GetChannelAsync(channelID);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Couldn't get Server Channel", "SDiscordServer.cs", ex);
                throw;
            }
        }
        public async ValueTask<DiscordRole> getRoleAsync(SDiscordRole type)
        {
            try
            {
                string role = string.Empty;
                switch (type)
                {
                    case SDiscordRole.MemberRole:
                        role = "memberroleid";
                        break;

                    default:
                        throw new Exception("Unknown role");
                }
                ulong roleID = (ulong)(await SDBEngine.SelectFromAsync("serverconfig", [role], new Dictionary<string, object> { { "serverid", Guild.Id } }))[0][0];

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

            var leaderboard = await SDBEngine.SelectFromAsync("serverconfig", ["userid", "points"], new Dictionary<string, object> { { "serverid", Guild.Id } }, "points", true, 10);

            int i = 0;
            foreach (var current in leaderboard)
            {
                i++;
                string name = $"**Platz {i}:** ";
                try
                {
                    DiscordUser user = await Guild.GetMemberAsync((ulong)current[0]);
                    name += user.Username;
                }
                catch
                {
                    name += $"[User unavailible!](https://discord.com/users/{current[0]})";
                }

                result.Add(name, (ulong)current[1]);                
            }

            return result;
        }
        public async ValueTask<Dictionary<string, ulong>> getMessagesLeaderboardAsync()
        {
            Dictionary<string, ulong> result = new();

            var leaderboard = await SDBEngine.SelectFromAsync("serverconfig", ["userid", "number"], new Dictionary<string, object> { { "serverid", Guild.Id } }, "number", true, 10);

            int i = 0;
            foreach (var current in leaderboard)
            {
                i++;
                string name = $"**Platz {i}:** ";
                try
                {
                    DiscordUser user = await Guild.GetMemberAsync((ulong)current[0]);
                    name += user.Username;
                }
                catch
                {
                    name += $"[User unavailible!](https://discord.com/users/{current[0]})";
                }

                result.Add(name, (ulong)current[1]);                
            }

            return result;
        }
    }
}