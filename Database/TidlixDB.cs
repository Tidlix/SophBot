using System.Data;
using DSharpPlus.Entities;
using Npgsql;
using SophBot.Configuration;
namespace SophBot.Database {
    public class TidlixDB {
        private static string connString = "errorConnString";
        private static NpgsqlConnection connection = new NpgsqlConnection();

        public static async ValueTask createDB() {
            await createConnection();
            await createTables();
        }
        private static async ValueTask createTables() {
            var cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS data; " +
            "CREATE TABLE IF NOT EXISTS data.warnings (warnid BIGSERIAL PRIMARY KEY, serverid BIGINT, userid BIGINT, reason VARCHAR(2000), date VARCHAR(22));" +
            "CREATE TABLE IF NOT EXISTS data.customcommands (serverid BIGINT, cmd VARCHAR(100), output VARCHAR(2000));" +
            "CREATE TABLE IF NOT EXISTS data.serverconfig (serverid BIGINT PRIMARY KEY, rulechannel BIGINT, welcomechannel BIGINT, memberrole BIGINT, mentionrole BIGINT);" +
            "CREATE TABLE IF NOT EXISTS data.twitchmonitorings (discordchannel BIGINT NOT NULL, twitchchannel VARCHAR NOT NULL)", 
            connection);
            await cmd.ExecuteNonQueryAsync();
        }
        private static async ValueTask createConnection() {
            connString = $"Host={Config.PSQL.Host};Database={Config.PSQL.Database};Username={Config.PSQL.Username};Password={Config.PSQL.Password};";
            connection = new NpgsqlConnection(connString);
            await connection.OpenAsync();
        }


        #region Serverconfig
        public static async ValueTask createServerconfig (ulong serverid, ulong rulechannel, ulong welcomeChannel, ulong memberRole, ulong mentionRole) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using (connection) {
                var cmd = new NpgsqlCommand($"INSERT INTO data.serverconfig (serverid, rulechannel, welcomechannel, memberrole, mentionrole) VALUES ({serverid}, {rulechannel}, {welcomeChannel}, {memberRole}, {mentionRole})", connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask modifyServerconfig (string target, ulong value, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using (connection) {
                var cmd = new NpgsqlCommand($"UPDATE data.serverconfig SET {target} = {value} WHERE serverid = {serverid}", connection);
                await cmd.ExecuteNonQueryAsync();
            }
            
        }
        public static async ValueTask deleteServerconfig (ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using (connection) {
                var cmd = new NpgsqlCommand($"DELETE FROM data.serverconfig WHERE serverid = {serverid}", connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask<ulong> readServerconfig (string target, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using (connection) {
                var cmd = new NpgsqlCommand($"SELECT {target} FROM data.serverconfig WHERE serverid = {serverid}", connection);
                var reader = await cmd.ExecuteReaderAsync();
                ulong result = 0;

                while (await reader.ReadAsync()) result = (ulong)reader.GetInt64(0);

                return result;
            }
        } 
        #endregion

        #region Customcommands
        public static async ValueTask<bool> checkCommandExists(string command, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using(connection) {
                var cmd = new NpgsqlCommand($"SELECT output FROM data.customcommands WHERE serverid = {serverid} AND cmd = '{command}'", connection);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync()) {
                    if (reader.GetString(0) == null) 
                        return false;
                    else return true;
                }
                return false;
            }
        }
        public static async ValueTask createCommand (string command, string output, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using(connection) {
                var cmd = new NpgsqlCommand($"Insert into data.customcommands (serverid, cmd, output) VALUES ({serverid}, '{command}', '{output}')", connection);
                var reader = await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask deleteCommand(string command, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using(connection) {
                var cmd = new NpgsqlCommand($"DELETE FROM data.customcommands WHERE serverid = {serverid} AND cmd = '{command}'", connection);
                var reader = await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask modifyCommand(string command, string newOutput, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using(connection) {
                var cmd = new NpgsqlCommand($"UPDATE data.customcommands SET output = '{newOutput}' WHERE serverid = {serverid} AND cmd = '{command}'", connection);
                var reader = await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask<string> getCommand(string command, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            using (connection){
                var cmd = new NpgsqlCommand($"SELECT output FROM data.customcommands WHERE serverid = {serverid} AND cmd = '{command}'", connection);
                var reader = await cmd.ExecuteReaderAsync();
                string result = "ERROR - Daten konnten nicht von Datenbank gelesen werden!";

                while (await reader.ReadAsync()) {
                    result = reader.GetString(0);
                }
                return result;
            }
        }
        public static async ValueTask<List<string>> getAllCommands(ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            using (connection){
                var cmd = new NpgsqlCommand($"SELECT cmd FROM data.customcommands WHERE serverid = {serverid}", connection);
                var reader = await cmd.ExecuteReaderAsync();
                List<string> result = new List<string>();

                while (await reader.ReadAsync()) {
                    result.Add(reader.GetString(0));
                }
                return result;
            }
        }
        #endregion

        #region Warnings
        public static async ValueTask createWarning (ulong serverid, ulong userid, string reason) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            
            using (connection) {
                DateTime dt = DateTime.Now;
                string dateFormat = "[dd.MM.yyyy - HH:mm] ";

                var cmd = new NpgsqlCommand($"INSERT INTO data.warnings (serverid, userid, reason, date) VALUES ({serverid}, {userid}, '{reason}', '{dt.ToString(dateFormat)}')", connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask deleteWarning (ulong warnid, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using (connection){
                var cmd = new NpgsqlCommand($"DELETE FROM data.warnings WHERE warnid = {warnid} AND guildid = {serverid}", connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask<string> getWarnings (ulong userid, ulong serverid) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            using (connection){
                var cmd = new NpgsqlCommand($"SELECT warnid, reason, date FROM data.warnings WHERE serverid = {serverid} AND userid = {userid}", connection);
                var reader = await cmd.ExecuteReaderAsync();
                string result = "**Datum/Zeit | Warn-ID | Grund** \n";

                while (await reader.ReadAsync()) {
                    result += $"> {reader.GetString(2)} | {reader.GetInt64(0)} | *{reader.GetString(1)}* \n";
                }
                return result;
            }
        }
        #endregion

        #region TwitchMonitorings
        public static async ValueTask newMonitoring (string twitchChannel, ulong discordChannelId) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            
            using (connection) {
                var cmd = new NpgsqlCommand($"INSERT INTO data.twitchmonitorings (discordchannel, twitchchannel) VALUES ({discordChannelId}, '{twitchChannel}')", connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        public static async ValueTask<List<string>> getChannelMonitorings (ulong discordChannelId) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            using (connection){
                var cmd = new NpgsqlCommand($"SELECT twitchchannel FROM data.twitchmonitorings WHERE discordchannel = {discordChannelId}", connection);
                var reader = await cmd.ExecuteReaderAsync();
                var result = new List<string>();

                while (await reader.ReadAsync()) {
                    result.Add(reader.GetString(0));
                }
                return result;
            }
        }
        public static async ValueTask<List<ulong>> getMonitoringChannels (string twitchChannel) {
            if (connection.State == ConnectionState.Closed) await createConnection();
            using (connection){
                var cmd = new NpgsqlCommand($"SELECT discordchannel FROM data.twitchmonitorings WHERE twitchchannel = '{twitchChannel}'", connection);
                var reader = await cmd.ExecuteReaderAsync();
                var result = new List<ulong>();

                while (await reader.ReadAsync()) {
                    result.Add((ulong)reader.GetInt64(0));
                }
                return result;
            }
        }
        public static async ValueTask<List<string>> getAllMonitorings () {
            if (connection.State == ConnectionState.Closed) await createConnection();
            using (connection){
                var cmd = new NpgsqlCommand($"SELECT twitchchannel FROM data.twitchmonitorings", connection);
                var reader = await cmd.ExecuteReaderAsync();
                var result = new List<string>();

                while (await reader.ReadAsync()) {
                    if (!result.Contains(reader.GetString(0)))
                        result.Add(reader.GetString(0));
                }
                return result;
            }
        }
        public static async ValueTask removeMonitoring (ulong discordChannelId) {
            if (connection.State == ConnectionState.Closed) await createConnection();

            using (connection){
                var cmd = new NpgsqlCommand($"DELETE FROM data.twitchmonitorings WHERE discordchannel = {discordChannelId}", connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        #endregion
    }
}