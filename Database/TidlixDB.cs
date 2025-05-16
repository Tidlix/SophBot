using System.Data;
using System.Diagnostics;
using DSharpPlus.Entities;
using Npgsql;
using SophBot.Configuration;
using SophBot.Messages;
namespace SophBot.Database {
    public class TidlixDB {
        public static string connString = "errorConnString";
        public static NpgsqlConnection connection = new NpgsqlConnection();

        public static async ValueTask createDB()
        {
            connString = $"Host={Config.PSQL.Host};Database={Config.PSQL.Database};Username={Config.PSQL.Username};Password={Config.PSQL.Password};";

            await createConnection();
            await createTables();
        }
        private static async ValueTask createTables()
        {
            var cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS data; " +
            "CREATE TABLE IF NOT EXISTS data.warnings (warnid BIGSERIAL PRIMARY KEY, serverid BIGINT, userid BIGINT, reason VARCHAR(2000), date VARCHAR(22));" +
            "CREATE TABLE IF NOT EXISTS data.customcommands (serverid BIGINT, cmd VARCHAR(100), output VARCHAR(2000));" +
            "CREATE TABLE IF NOT EXISTS data.serverconfig (serverid BIGINT PRIMARY KEY, rulechannel BIGINT, welcomechannel BIGINT, memberrole BIGINT, mentionrole BIGINT);" +
            "CREATE TABLE IF NOT EXISTS data.twitchmonitorings (discordchannel BIGINT NOT NULL, twitchchannel VARCHAR NOT NULL);" +
            "CREATE TABLE IF NOT EXISTS data.userprofiles (userid BIGINT, serverid BIGINT, points BIGINT, checkvalue VARCHAR(1));", 
            connection);
            await cmd.ExecuteNonQueryAsync();
        }
        private static async ValueTask createConnection()
        {
            connection = new NpgsqlConnection(connString);
            await connection.OpenAsync();          
        }


        #region Serverconfig
        public static class ServerConfig {
            public static async ValueTask createAsnyc (ulong serverid, ulong rulechannel, ulong welcomeChannel, ulong memberRole, ulong mentionRole) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection) {
                    var cmd = new NpgsqlCommand($"INSERT INTO data.serverconfig (serverid, rulechannel, welcomechannel, memberrole, mentionrole) VALUES ({serverid}, {rulechannel}, {welcomeChannel}, {memberRole}, {mentionRole})", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask modifyValueAsnyc (string target, string value, ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection) {
                    var cmd = new NpgsqlCommand($"UPDATE data.serverconfig SET {target} = {value} WHERE serverid = {serverid}", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
                
            }
            public static async ValueTask deleteAsync (ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection) {
                    var cmd = new NpgsqlCommand($"DELETE FROM data.serverconfig WHERE serverid = {serverid}", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask<ulong> readValueAsync (string target, ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection) {
                    var cmd = new NpgsqlCommand($"SELECT {target} FROM data.serverconfig WHERE serverid = {serverid}", connection);
                    var reader = await cmd.ExecuteReaderAsync();
                    ulong result = 0;

                    while (await reader.ReadAsync()) result = (ulong)reader.GetInt64(0);

                    return result;
                }
            }
        }
         
        #endregion

        #region Customcommands
        public static class CustomCommands {
            public static async ValueTask<bool> checkExistanceAsync(string command, ulong serverid) {
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
            public static async ValueTask createAsnyc (string command, string output, ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using(connection) {
                    var cmd = new NpgsqlCommand($"Insert into data.customcommands (serverid, cmd, output) VALUES ({serverid}, '{command}', '{output}')", connection);
                    var reader = await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask deleteAsnyc(string command, ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using(connection) {
                    var cmd = new NpgsqlCommand($"DELETE FROM data.customcommands WHERE serverid = {serverid} AND cmd = '{command}'", connection);
                    var reader = await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask modifyAsync(string command, string newOutput, ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using(connection) {
                    var cmd = new NpgsqlCommand($"UPDATE data.customcommands SET output = '{newOutput}' WHERE serverid = {serverid} AND cmd = '{command}'", connection);
                    var reader = await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask<string> getCommandAsnyc(string command, ulong serverid) {
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
            public static async ValueTask<List<string>> getAllCommandsAsnyc(ulong serverid) {
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
        }
        
        #endregion

        #region Warnings
        public static class Warnings {
            public static async ValueTask createAsnyc (ulong serverid, ulong userid, string reason) {
                if (connection.State == ConnectionState.Closed) await createConnection();
                
                using (connection) {
                    DateTime dt = DateTime.Now;
                    string dateFormat = "[dd.MM.yyyy - HH:mm] ";

                    var cmd = new NpgsqlCommand($"INSERT INTO data.warnings (serverid, userid, reason, date) VALUES ({serverid}, {userid}, '{reason}', '{dt.ToString(dateFormat)}')", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask deleteAsnyc (ulong warnid, ulong serverid) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection){
                    var cmd = new NpgsqlCommand($"DELETE FROM data.warnings WHERE warnid = {warnid} AND guildid = {serverid}", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask<string> getAllByUserAsnyc (ulong userid, ulong serverid) {
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
        }
        
        #endregion

        #region TwitchMonitorings
        public static class TwitchMonitorings {
            public static async ValueTask createAsnyc (string twitchChannel, ulong discordChannelId) {
                if (connection.State == ConnectionState.Closed) await createConnection();
                
                using (connection) {
                    var cmd = new NpgsqlCommand($"INSERT INTO data.twitchmonitorings (discordchannel, twitchchannel) VALUES ({discordChannelId}, '{twitchChannel}')", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask deleteAsnyc (ulong discordChannelId) {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection){
                    var cmd = new NpgsqlCommand($"DELETE FROM data.twitchmonitorings WHERE discordchannel = {discordChannelId}", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask<List<string>> getChannelMonitoringsAsnyc (ulong discordChannelId) {
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
            public static async ValueTask<List<ulong>> getMonitoringChannelsAsync (string twitchChannel) {
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
            public static async ValueTask<List<string>> getAllMonitoringsAsync () {
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
        }


        #endregion

        #region UserProfiles
        public static class UserProfiles
        {
            public static async ValueTask createAsync(ulong serverid, ulong userid, long points)
            {
                if (await checkExistanceAsnyc(serverid, userid)) throw new Exception("User Profile allready exists!");
                if (connection.State == ConnectionState.Closed) await createConnection();


                using (connection)
                {
                    var cmd = new NpgsqlCommand($"INSERT INTO data.userprofiles (userid, serverid, points, checkvalue) VALUES ({userid}, {serverid}, {points}, 'x')", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask modifyValueAsnyc(string target, string value, ulong serverid, ulong userid)
            {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection)
                {
                    var cmd = new NpgsqlCommand($"UPDATE data.userprofiles SET {target} = {value} WHERE serverid = {serverid} AND userid = {userid}", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            public static async ValueTask<long> getPointsAsync(ulong serverid, ulong userid)
            {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection)
                {
                    var cmd = new NpgsqlCommand($"SELECT points FROM data.userprofiles WHERE serverid = {serverid} AND userid = '{userid}'", connection);
                    var reader = await cmd.ExecuteReaderAsync();

                    long result = -404;

                    while (await reader.ReadAsync())
                    {
                        result = reader.GetInt64(0);
                    }

                    return result;
                }
            }
            public static async ValueTask<bool> checkExistanceAsnyc(ulong serverid, ulong userid)
            {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection)
                {
                    var cmd = new NpgsqlCommand($"SELECT checkvalue FROM data.userprofiles WHERE serverid = {serverid} AND userid = {userid}", connection);
                    var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        if (reader.GetString(0) == null)
                            return false;
                        else return true;
                    }
                    return false;
                }
            }
            public static async ValueTask<int> getLeaderBoardScoreAsync(ulong serverid, ulong userid)
            {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection)
                {
                    var cmd = new NpgsqlCommand($"SELECT userid FROM data.userprofiles WHERE serverid = {serverid} ORDER BY points DESC", connection);
                    var reader = await cmd.ExecuteReaderAsync();
                    int i = 1;

                    while (await reader.ReadAsync())
                    {
                        ulong currentId = (ulong)reader.GetInt64(0);

                        if (currentId == userid) break;
                        i++;
                    }
                    return i;
                }
            }
            public static async ValueTask<int> getLeaderBoardCountAsync(ulong serverid)
            {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection)
                {
                    var cmd = new NpgsqlCommand($"SELECT COUNT(points) FROM data.userprofiles WHERE serverid = {serverid}", connection);
                    var reader = await cmd.ExecuteReaderAsync();
                    int i = 1;

                    while (await reader.ReadAsync())
                    {
                        i = reader.GetInt16(0);
                    }
                    return i;
                }
            }
            public static async ValueTask<Dictionary<ulong, long>> getLeaderBoardTopAsync(ulong serverid, int numberOfRows)
            {
                if (connection.State == ConnectionState.Closed) await createConnection();

                using (connection)
                {
                    var cmd = new NpgsqlCommand($"SELECT userid, points FROM data.userprofiles WHERE serverid = {serverid} ORDER BY points DESC LIMIT {numberOfRows}", connection);
                    var reader = await cmd.ExecuteReaderAsync();

                    Dictionary<ulong, long> result = new();

                    while (await reader.ReadAsync())
                    {
                        ulong currentId = (ulong)reader.GetInt64(0);
                        long currentPoints = reader.GetInt64(1);

                        result.Add(currentId, currentPoints);
                    }
                    return result;
                }
            }
        }
        #endregion
    }
}