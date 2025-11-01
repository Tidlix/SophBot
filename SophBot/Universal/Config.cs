namespace SophBot.Universal
{
    public class Config
    {
#pragma warning disable CS8618
        public static class Discord
        {
            public static string Token { get; set; }
        }
        public static class Twitch
        {
            public static string ClientId { get; set; }
            public static string ClientSecret { get; set; }
        }
        public static class Database
        {
            public static string Host { get; set; }
            public static string Port { get; set; }
            public static string DatabaseName { get; set; }
            public static string User { get; set; }
            public static string Password { get; set; }
            public static string Schema { get; set; }
        }
        public static class Ai
        {
            public static string Token { get; set; }
        }
#pragma warning restore CS8618

        public static void Read()
        {
            Discord.Token = getValue(ConfigPath.Discord, "Token");

            Twitch.ClientId = getValue(ConfigPath.Twitch, "ClientId");
            Twitch.ClientSecret = getValue(ConfigPath.Twitch, "ClientSecret");

            Database.Host = getValue(ConfigPath.Database, "Host");
            Database.Port = getValue(ConfigPath.Database, "Port");
            Database.DatabaseName = getValue(ConfigPath.Database, "Database");
            Database.User = getValue(ConfigPath.Database, "Username");
            Database.Password = getValue(ConfigPath.Database, "Password");
            Database.Schema = getValue(ConfigPath.Database, "Schema");

            Ai.Token = getValue(ConfigPath.Ai, "Token");
        }
        internal class ConfigPath
        {
            public static string Discord = $"{AppDomain.CurrentDomain.BaseDirectory}/config/discord.conf";
            public static string Twitch = $"{AppDomain.CurrentDomain.BaseDirectory}/config/twitch.conf";
            public static string Database = $"{AppDomain.CurrentDomain.BaseDirectory}/config/database.conf";
            public static string Ai = $"{AppDomain.CurrentDomain.BaseDirectory}/config/ai.conf";
        }
        private static string getValue(string filePath, string key)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Config file not found: {filePath}");

            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                {
                    var k = parts[0].Trim();
                    var v = parts[1].Trim().Trim('"'); // remove surrounding quotes if present
                    if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                        return v;
                }
            }

            return string.Empty;
        }

    }
}