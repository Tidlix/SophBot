using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.conf
{
    public class SConfig
    {
        public static LogLevel LogLevel;
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
            public static string GeminiKey;
            public static string GroqKey;
            public static string SystemInstructions;
        }

        public static async ValueTask ReadConfigAsync()
        {
            try
            {
                StreamReader sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/discord.conf");
                SLogger.Log(LogLevel.Debug, $"Reading file: discord.conf", "SConfig.cs");
                string confFile = await sr.ReadToEndAsync();
                SLogger.Log(LogLevel.Debug, confFile, "discord.conf");

                Discord.Token = getValue(confFile, "Token");


                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/database.conf");
                SLogger.Log(LogLevel.Debug, $"Reading file: database.conf", "SConfig.cs");
                confFile = await sr.ReadToEndAsync();
                SLogger.Log(LogLevel.Debug, confFile, "database.conf");

                Database.Host = getValue(confFile, "Host");
                Database.Port = getValue(confFile, "Port");
                Database.DB_Name = getValue(confFile, "Database");
                Database.Schema = getValue(confFile, "Schema");
                Database.Username = getValue(confFile, "Username");
                Database.Password = getValue(confFile, "Password");


                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/twitch.conf");
                SLogger.Log(LogLevel.Debug, $"Reading file: twitch.conf", "SConfig.cs");
                confFile = await sr.ReadToEndAsync();
                SLogger.Log(LogLevel.Debug, confFile, "twitch.conf");

                Twitch.ClientId = getValue(confFile, "ID");
                Twitch.ClientSecret = getValue(confFile, "Secret");


                sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config/ai.conf");
                SLogger.Log(LogLevel.Debug, $"Reading file: ai.conf", "SConfig.cs");
                confFile = await sr.ReadToEndAsync();
                SLogger.Log(LogLevel.Debug, confFile, "ai.conf");

                AI.GeminiKey = getValue(confFile, "Gemini_Key");
                AI.GroqKey = getValue(confFile, "Groq_Key");
                AI.SystemInstructions = getValue(confFile, "Sys_Instruction");
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Critical, "Failed to read config files", "SConfig.cs", ex);
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