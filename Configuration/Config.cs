using Newtonsoft.Json;

namespace SophBot.Configuration {
    public class Config {
        private static string configFile = $"{AppDomain.CurrentDomain.BaseDirectory}/config.json";

        public static string Token = "errorToken";
        public static string Psql_host = "errorSQL";
        public static string Psql_database = "errorSQL";
        public static string Psql_username = "errorSQL";
        public static string Psql_password = "errorSQL";

        public static async ValueTask ReadAsnyc() {
            try {
                using (StreamReader sr = new StreamReader(configFile)) {
                    var file = await sr.ReadToEndAsync();

                    if (file == null) throw new Exception($"Coulnd't read config file from {configFile}");

                    var data = JsonConvert.DeserializeObject<configStructure>(file);

                    if (data == null) throw new Exception("Coulnd't convert config data");

                    Token = data.token;
                    Psql_host = data.psql_host;
                    Psql_database = data.psql_database;
                    Psql_username = data.psql_username;
                    Psql_password = data.psql_password;
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }

        private class configStructure {
            public string token = "errorStructureToken";
            public string psql_host = "errorSQL";
            public string psql_username = "errorSQL";
            public string psql_database = "errorSQL";
            public string psql_password = "errorSQL";
        }
    }
}