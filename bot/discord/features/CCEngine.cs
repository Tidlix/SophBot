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
                var values = new Dictionary<string, object>
                {
                    { "serverid", Guild.Id.ToString() },
                    { "command", command },
                    { "value", output }
                };

                await SDBEngine.InsertToAsync("customcommands", new Dictionary<string, object>[] { values });
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
                var values = new Dictionary<string, object>
                {
                    { "value", output }
                };

                var conditions = new Dictionary<string, object>
                {
                    { "serverid", Guild.Id.ToString() },
                    { "command", command }
                };

                await SDBEngine.ModifyAtAsync("customcommands", values, conditions);
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
                var conditions = new Dictionary<string, object>
                {
                    { "serverid", Guild.Id.ToString() },
                    { "command", command }
                };

                await SDBEngine.DeleteFromAsync("customcommands", conditions);
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
                var conditions = new Dictionary<string, object>
                {
                    { "serverid", Guild.Id.ToString() },
                    { "command", command }
                };

                var select = await SDBEngine.SelectFromAsync("customcommands", new string[] { "value" }, conditions, limit: 1);
                var response = (select == null || select.Length == 0) ? "" : select.First()[0]?.ToString() ?? "";

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
                var conditions = new Dictionary<string, object>
                {
                    { "serverid", Guild.Id.ToString() }
                };

                var select = await SDBEngine.SelectFromAsync("customcommands", new string[] { "command" }, conditions);
                return select?.Select(r => r[0]?.ToString() ?? "").ToList();
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Something went wrong while selecting CC list", "SDiscordServer.cs", ex);
                throw;
            }
        }
    }
}
