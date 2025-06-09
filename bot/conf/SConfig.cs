using System.Text.RegularExpressions;
using SophBot.bot.logs;

namespace SophBot.bot.conf
{
    public class SConfig
    {
        public static LogType LogLevel;
        public static class Discord
        {
#pragma warning disable CS8618
            public static string Token;
        }
        public static class Database
        {
#pragma warning disable CS8618
            public static string Host;
            public static string Port;
            public static string DB_Name;
            public static string Schema;
            public static string Username;
            public static string Password;
        }
        public static class Twitch
        {
#pragma warning disable CS8618
            public static string ClientId;
            public static string ClientSecret;
        }
        public static class AI
        {
#pragma warning disable CS8618
            public static string ApiKey;
        }

        public static async ValueTask ReadConfigAsync()
        {
            try
            {
                StreamReader sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/discord.conf");
                SLogger.Log($"Reading file: discord.conf", type: LogType.Debug);
                string confFile = await sr.ReadToEndAsync();
                SLogger.Log(confFile, type: LogType.Debug);

                Discord.Token = getValue(confFile, "Token");


                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/database.conf");
                SLogger.Log($"Reading file: database.conf", type: LogType.Debug);
                confFile = await sr.ReadToEndAsync();
                SLogger.Log(confFile, type: LogType.Debug);

                Database.Host = getValue(confFile, "Host");
                Database.Port = getValue(confFile, "Port");
                Database.DB_Name = getValue(confFile, "Database");
                Database.Schema = getValue(confFile, "Schema");
                Database.Username = getValue(confFile, "Username");
                Database.Password = getValue(confFile, "Password");


                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/twitch.conf");
                SLogger.Log($"Reading file: twitch.conf", type: LogType.Debug);
                confFile = await sr.ReadToEndAsync();
                SLogger.Log(confFile, type: LogType.Debug);

                Twitch.ClientId = getValue(confFile, "ID");
                Twitch.ClientSecret = getValue(confFile, "Secret");


                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/ai.conf");
                SLogger.Log($"Reading file: ai.conf", type: LogType.Debug);
                confFile = await sr.ReadToEndAsync();
                SLogger.Log(confFile, type: LogType.Debug);

                AI.ApiKey = getValue(confFile, "API_Key");


            }
            catch (Exception ex)
            {
                SLogger.Log("Failed to read config files", "SConfig.cs -> ReadConfigAsnyc()", ex, LogType.Critical);
            }
        }

        private static string getValue(string file, string target)
        {
            string pattern = @$"^{target}\s+""([^""]+)"";";

            Match match = Regex.Match(file, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string result = match.Groups[1].Value;
                if (result == "changeMe") throw new Exception($"Target ({target}) is default value. Please change");
                return result;
            }
            else
            {
                throw new Exception($"Target ({target}) couldn't be found in file!");
            }
        }
    }
}