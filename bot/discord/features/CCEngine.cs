using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.logs;

namespace SophBot.bot.discord.features
{
    public class CCEngine
    {
        private DiscordGuild Guild;
        public CCEngine(DiscordGuild guild)
        {
            this.Guild = guild;
        }



        public async ValueTask createAsync(string command, string output)
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
        public async ValueTask modifyAsync(string command, string output)
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
        public async ValueTask deleteAsync(string command)
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
        public async ValueTask<string> getOutputAsync(string command)
        {
            try
            {
                List<SDBValue> conditions = new();
                conditions.Add(new(SDBColumn.ServerID, Guild.Id.ToString()));
                conditions.Add(new(SDBColumn.Name, command));

                string response;
                var select = await SDBEngine.SelectAsync(SDBTable.CustomCommands, SDBColumn.Description, conditions, limit: 1);
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
        public async ValueTask<List<string>?> getListAsync()
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