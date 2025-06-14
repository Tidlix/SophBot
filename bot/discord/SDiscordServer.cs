using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.logs;

namespace SophBot.bot.discord
{
    public enum SDiscordChannelType {
        WelcomeChannel, 
        RuleChannel
    }

    public class SDiscordServer
    {
        public DiscordGuild Guild;

        public SDiscordServer(DiscordGuild guild)
        {
            this.Guild = guild;
        }


        public async ValueTask<DiscordChannel> getChannelAsync(SDiscordChannelType type)
        {
            try
            {
                ulong channelID;
                switch (type)
                {
                    case SDiscordChannelType.WelcomeChannel:
                        ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.ServerConfig, SDBColumn.WelcomeChannelID)).First(), out channelID);
                        break;
                    case SDiscordChannelType.RuleChannel:
                        ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.ServerConfig, SDBColumn.RuleChannelID)).First(), out channelID);
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
    }
}