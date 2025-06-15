using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
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

        public SDiscordServer(DiscordGuild guild)
        {
            this.Guild = guild;
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

        public async ValueTask createCCAsync(string command, string output)
        {
            try
            {
                List<SDBValue> values = new();
                values.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                values.Add(new(SDBColumn.Name, command));
                values.Add(new(SDBColumn.Description, output));
                await SDBEngine.InsertAsync(values, SDBTable.CustomCommands);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Couldn't insert data - Data already exists like that in db")
                {
                    SLogger.Log(LogLevel.Warning, "Something went wrong while creating new CC", "SDiscordServer.cs", ex);
                    return;
                }
                SLogger.Log(LogLevel.Error, "Something went wrong while creating new CC", "SDiscordServer.cs", ex);
                throw;
            }
        }
        public async ValueTask modifyCCAsync(string command, string output)
        {
            try
            {
                List<SDBValue> values = new();
                values.Add(new(SDBColumn.Description, output));

                List<SDBValue> conditions = new();
                conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                conditions.Add(new(SDBColumn.Name, command));

                await SDBEngine.ModifyAsync(values, SDBTable.CustomCommands, conditions);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Something went wrong while modifying CC", "SDiscordServer.cs", ex);
                throw;
            }
        }
        public async ValueTask deleteCCAsync(string command)
        {
            try
            {
                List<SDBValue> conditions = new();
                conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                conditions.Add(new(SDBColumn.Name, command));

                await SDBEngine.DeleteAsync(SDBTable.CustomCommands, conditions);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Something went wrong while deleting CC", "SDiscordServer.cs", ex);
                throw;
            }
        }
        public async ValueTask<string> getCCOutputAsync(string command)
        {
            try
            {
                List<SDBValue> conditions = new();
                conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                conditions.Add(new(SDBColumn.Name, command));

                string response;
                var select = await SDBEngine.SelectAsync(SDBTable.CustomCommands, SDBColumn.Description, conditions, 1);
                response = (select == null) ? "" : select.First();

                return response;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Value cannot be null")) return "";
                SLogger.Log(LogLevel.Error, "Something went wrong while selecting CC output", "SDiscordServer.cs", ex);
                return "";
            }
        }
        public async ValueTask<List<string>?> getCCListAsync()
        {
            try
            {

                List<SDBValue> conditions = new();
                conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));

                return await SDBEngine.SelectAsync(SDBTable.CustomCommands, SDBColumn.Name, conditions);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Something went wrong while selecting CC list", "SDiscordServer.cs", ex);
                throw;
            }            
        }
    }
}