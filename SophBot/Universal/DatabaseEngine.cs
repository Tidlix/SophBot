using System.Data;
using System.Data.Common;
using Npgsql;
using Npgsql.Schema;

namespace SophBot.Universal
{
    public static class DatabaseEngine
    {
        #region Variables / Initialization
        private static string connStr = "null";
        private static string schema = "test";

        public static void Initialize()
        {
            connStr = $"host={Config.Database.Host}; port={Config.Database.Port}; database={Config.Database.DatabaseName}; username={Config.Database.User}; password={Config.Database.Password};";
            schema = Config.Database.Schema;
        }
        #endregion

        #region DB-Tools
        public enum DBTable
        {
            // NC - Not Created in DB yet!
            Profiles,       // id ; discord-id ; twitch-id ; discord-messages ; twitch-messages ; channel-points 
            Servers,        // NC  server-id ; 
            Commands,       // NC  command ; content
            Wiki,           // article ; site ; content
            Monitorings,    // NC  discord-channel ; twitch-channel ; mention-role
            AI_Memory,      // id ; content
            Logs            // id ; datetime ; loglevel ; content ; source
        }
        public record DBCondition(string Column, string Operator, object Value);
        
        private static object ConvertParameterValue(object value)
        {
            if (value == null)
                return DBNull.Value;
            
            if (value is ulong ulongValue)
                return (long)ulongValue;

            
            return value;
        }

        private static string TableString(DBTable table)
        {
            string result = schema + ".";
            switch (table)
            {
                case DBTable.Profiles:
                    return result + "\"profiles\"";
                case DBTable.Wiki:
                    return result + "\"wiki\"";
                case DBTable.AI_Memory:
                    return result + "\"ai-memory\"";
                case DBTable.Logs:
                    return result + "\"logs\"";
                case DBTable.Monitorings:
                    return result + "\"monitorings\"";
                default:
                    throw new NotImplementedException($"Table ({table}) not implemented yet!");
            }
        }

        private static DataTable ExecuteReader(NpgsqlCommand cmd)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    cmd.Connection = conn;

                    using (var reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to Execute DB-Reader. - {ex.Message}"); 
            }
        }

        private static void ExecuteQuery(NpgsqlCommand cmd)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    //Console.WriteLine(cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to Execute DB-Query. - {ex.Message}");
            }
        }
        #endregion

        #region Select
        public static DataTable SelectTable(DBTable table, string? orderByColumn = null)
        {
            var cmd = new NpgsqlCommand($"SELECT * FROM {TableString(table)}" + (orderByColumn is null ? "" : $" ORDER BY \"{orderByColumn}\""));
            return ExecuteReader(cmd);
        }
        
        public static DataTable SelectTopEntrys(DBTable table, int limit, string? orderByColumn = null, bool desc = false)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.CommandText = $@"
            SELECT * 
            FROM {TableString(table)} 
            {(orderByColumn is null ? "" : $"ORDER BY \"{orderByColumn}\"") + (orderByColumn is not null && desc ? " DESC " : " ASC ")} 
            LIMIT {limit}";
            return ExecuteReader(cmd);
        }
        
        public static DataTable SelectEntrys(DBTable table, IEnumerable<string> columns, IEnumerable<DBCondition> conditions)
        {
            using var cmd = new NpgsqlCommand();

            string columnStr = string.Join(", ", columns.Select(column => $"\"{column}\""));
            
            List<string> conditionList = new();
            int i = 0;

            foreach (var current in conditions)
            {
                string paramName = $"@p{i}";
                conditionList.Add($"\"{current.Column}\" {current.Operator} {paramName}");
                cmd.Parameters.AddWithValue(paramName, ConvertParameterValue(current.Value));
                i++;
            }

            string conditionStr = string.Join(" AND ", conditionList);
            cmd.CommandText = $@"
                SELECT {columnStr}
                FROM {TableString(table)}
                {(conditions.Any() ? $"WHERE {conditionStr}" : string.Empty)}";

            return ExecuteReader(cmd);
        }
        #endregion

        #region Insert / Modify / Delete
        public static void InsertData(DBTable table, Dictionary<string, object> data)
        {
            using var cmd = new NpgsqlCommand();

            var columns = data.Keys.ToArray();
            var columnList = string.Join(", ", columns.Select(c => $"\"{c}\""));
            var paramList = string.Join(", ", columns.Select((c, i) => $"@p{i}"));

            int i = 0;
            foreach (var current in data)
            {
                cmd.Parameters.AddWithValue($"@p{i}", ConvertParameterValue(current.Value));
                i++;
            }

            cmd.CommandText = $@"
            INSERT INTO {TableString(table)} ({columnList})
            VALUES ({paramList});";

            ExecuteQuery(cmd);
        }
        
        public static void ModifyData(DBTable table, Dictionary<string, object> data, IEnumerable<DBCondition> conditions)
        {
            using var cmd = new NpgsqlCommand();

            List<string> dataList = new();
            List<string> conditionList = new();


            int i = 0;
            foreach (var current in data)
            {
                cmd.Parameters.AddWithValue($"@d{i}", ConvertParameterValue(current.Value));
                dataList.Add($"\"{current.Key}\" = @d{i}");
                i++;
            }

            int j = 0;
            foreach (var current in conditions)
            {
                string paramName = $"@p{j}";
                conditionList.Add($"\"{current.Column}\" {current.Operator} {paramName}");
                cmd.Parameters.AddWithValue(paramName, ConvertParameterValue(current.Value));
                j++;
            }

            string dataStr = string.Join(", ", dataList);
            string conditionStr = string.Join(" AND ", conditionList);


            cmd.CommandText = $@"
            UPDATE {TableString(table)}
            SET {dataStr}
            WHERE {conditionStr};";

            ExecuteQuery(cmd);
        }
        
        public static void DeleteData(DBTable table, IEnumerable<DBCondition> conditions)
        {
            NpgsqlCommand cmd = new();
            List<string> conditionList = new();
            int i = 0;

            foreach (var current in conditions)
            {
                string paramName = $"@p{i}";
                conditionList.Add($"\"{current.Column}\" {current.Operator} {paramName}");
                cmd.Parameters.AddWithValue(paramName, ConvertParameterValue(current.Value));
                i++;
            }

            cmd.CommandText = $"DELETE FROM {TableString(table)} WHERE {string.Join(" AND ", conditionList)}";

            ExecuteQuery(cmd);
        }
        #endregion
    }

}