using System.Text.RegularExpressions;
using SophBot.bot.logs;

namespace SophBot.bot.conf
{
    public class SConfig
    {
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
                string confFile = await sr.ReadToEndAsync();
                Discord.Token = getValue(confFile, "Token");

                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/database.conf");
                confFile = await sr.ReadToEndAsync();
                Database.Host = getValue(confFile, "Host");
                Database.Port = getValue(confFile, "Port");
                Database.DB_Name = getValue(confFile, "Database");
                Database.Username = getValue(confFile, "Username");
                Database.Password = getValue(confFile, "Password");

                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/twitch.conf");
                confFile = await sr.ReadToEndAsync();
                Twitch.ClientId = getValue(confFile, "ID");
                Twitch.ClientSecret = getValue(confFile, "Secret");

                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/ai.conf");
                confFile = await sr.ReadToEndAsync();
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