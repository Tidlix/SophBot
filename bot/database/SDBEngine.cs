using Npgsql;
using SophBot.bot.conf;
using SophBot.bot.logs;

namespace SophBot.bot.database
{
    public enum SDBTable
    {
        ServerConfig,
        UserProfiles,
        CustomCommands,
        TwitchMonitorings,
        Warnings,
        Wiki
    }

    public class SDBEngine
    {
        #region Variables
#pragma warning disable CS8618
        private static string connString;
        private static NpgsqlConnection _Connection;
#pragma warning disable CS8618

        private static string getColumnString(SDBColumn column)
        {
            switch (column)
            {
                case SDBColumn.UserID:
                    return "userid";
                case SDBColumn.Points:
                    return "points";
                case SDBColumn.ServerID:
                    return "server";
                case SDBColumn.RuleChannelID:
                    return "rulechannel";
                case SDBColumn.WelcomeChannelID:
                    return "welcomechannel";
                case SDBColumn.NotificationChannelID:
                    return "notificationchannel";
                case SDBColumn.TwitchChannel:
                    return "twitchchannel";
                case SDBColumn.MemberRoleID:
                    return "memberrole";
                case SDBColumn.MentionRoleID:
                    return "mentionrole";
                case SDBColumn.Name:
                    return "name";
                case SDBColumn.Number:
                    return "number";
                case SDBColumn.Description:
                    return "description";
                case SDBColumn.DateTime:
                    return "date";

                default:
                    throw new Exception("Column-Switch-Case not defined!");
            }
        }
        private static string getTableString(SDBTable table)
        {
            switch (table)
            {
                case SDBTable.ServerConfig:
                    return "serverconfig";
                case SDBTable.UserProfiles:
                    return "profiles";
                case SDBTable.TwitchMonitorings:
                    return "twitchmonitorings";
                case SDBTable.CustomCommands:
                    return "customcommands";
                case SDBTable.Warnings:
                    return "warnings";
                case SDBTable.Wiki:
                    return "wiki";

                default:
                    throw new Exception("Table-Switch-Case not defined!");
            }
        }
        #endregion

        #region  ColumnChecks
        internal static class ColumnList
        {
            public static SDBColumn[] ServerConfig =
            {
                SDBColumn.ServerID,
                SDBColumn.RuleChannelID,
                SDBColumn.WelcomeChannelID,
                SDBColumn.MemberRoleID
            };
            public static SDBColumn[] UserProfiles =
            {
                SDBColumn.ServerID,
                SDBColumn.UserID,
                SDBColumn.Points
            };
            public static SDBColumn[] CustomCommands =
            {
                SDBColumn.ServerID,
                SDBColumn.Name,
                SDBColumn.Description
            };
            public static SDBColumn[] TwitchMonitorings =
            {
                SDBColumn.ServerID,
                SDBColumn.TwitchChannel,
                SDBColumn.NotificationChannelID,
                SDBColumn.MentionRoleID
            };
            public static SDBColumn[] Warnings =
            {
                SDBColumn.ServerID,
                SDBColumn.UserID,
                SDBColumn.Description,
                SDBColumn.DateTime
            };
            public static SDBColumn[] Wiki = {
                SDBColumn.Name,
                SDBColumn.Number,
                SDBColumn.Description,
            };
        }
        private static void checkColumns(SDBTable table, List<SDBValue> values)
        {
            switch (table)
            {
                case SDBTable.ServerConfig:
                    foreach (SDBValue value in values)
                    {
                        SDBColumn column = value.Column;
                        if (!ColumnList.ServerConfig.Contains(column)) throw new Exception($"Table 'ServerCofig' does not contain column '{getColumnString(column)}'");
                    }
                    break;
                case SDBTable.UserProfiles:
                    foreach (SDBValue value in values)
                    {
                        SDBColumn column = value.Column;
                        if (!ColumnList.UserProfiles.Contains(column)) throw new Exception($"Table 'UserProfiles' does not contain column '{getColumnString(column)}'");
                    }
                    break;
                case SDBTable.CustomCommands:
                    foreach (SDBValue value in values)
                    {
                        SDBColumn column = value.Column;
                        if (!ColumnList.CustomCommands.Contains(column)) throw new Exception($"Table 'CustomCommands' does not contain column '{getColumnString(column)}'");
                    }
                    break;
                case SDBTable.TwitchMonitorings:
                    foreach (SDBValue value in values)
                    {
                        SDBColumn column = value.Column;
                        if (!ColumnList.TwitchMonitorings.Contains(column)) throw new Exception($"Table 'TwitchMonitorings' does not contain column '{getColumnString(column)}'");
                    }
                    break;
                case SDBTable.Warnings:
                    foreach (SDBValue value in values)
                    {
                        SDBColumn column = value.Column;
                        if (!ColumnList.Warnings.Contains(column)) throw new Exception($"Table 'Warnings' does not contain column '{getColumnString(column)}'");
                    }
                    break;
                case SDBTable.Wiki:
                    foreach (SDBValue value in values)
                    {
                        SDBColumn column = value.Column;
                        if (!ColumnList.Wiki.Contains(column)) throw new Exception($"Table 'Wiki' does not contain column '{getColumnString(column)}'");
                    }
                    break;
            }
        }
        #endregion


        #region Initialization
        public static async ValueTask Initialize()
        {
            connString = $"Host={SConfig.Database.Host}:{SConfig.Database.Port};Database ={SConfig.Database.DB_Name};Username={SConfig.Database.Username};Password={SConfig.Database.Password};";
            _Connection = new NpgsqlConnection(connString);
            await _Connection.OpenAsync();
            await createTables();
        }
        private static async ValueTask createTables()
        {
            try
            {
                List<string> columnList = new();
                NpgsqlCommand cmd = new();
                cmd.Connection = _Connection;

                foreach (SDBColumn column in ColumnList.ServerConfig) columnList.Add($"{getColumnString(column)} VARCHAR");
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {SConfig.Database.Schema}.{getTableString(SDBTable.ServerConfig)} ({string.Join(", ", columnList)})";
                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                columnList.Clear();
                await cmd.ExecuteNonQueryAsync();

                foreach (SDBColumn column in ColumnList.UserProfiles) columnList.Add($"{getColumnString(column)} VARCHAR");
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {SConfig.Database.Schema}.{getTableString(SDBTable.UserProfiles)} ({string.Join(", ", columnList)})";
                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                columnList.Clear();
                await cmd.ExecuteNonQueryAsync();

                foreach (SDBColumn column in ColumnList.Warnings) columnList.Add($"{getColumnString(column)} VARCHAR");
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {SConfig.Database.Schema}.{getTableString(SDBTable.Warnings)} ({string.Join(", ", columnList)})";
                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                columnList.Clear();
                await cmd.ExecuteNonQueryAsync();

                foreach (SDBColumn column in ColumnList.CustomCommands) columnList.Add($"{getColumnString(column)} VARCHAR");
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {SConfig.Database.Schema}.{getTableString(SDBTable.CustomCommands)} ({string.Join(", ", columnList)})";
                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                columnList.Clear();
                await cmd.ExecuteNonQueryAsync();

                foreach (SDBColumn column in ColumnList.TwitchMonitorings) columnList.Add($"{getColumnString(column)} VARCHAR");
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {SConfig.Database.Schema}.{getTableString(SDBTable.TwitchMonitorings)} ({string.Join(", ", columnList)})";
                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                columnList.Clear();
                await cmd.ExecuteNonQueryAsync();

                foreach (SDBColumn column in ColumnList.Wiki) columnList.Add($"{getColumnString(column)} VARCHAR");
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {SConfig.Database.Schema}.{getTableString(SDBTable.Wiki)} ({string.Join(", ", columnList)})";
                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                columnList.Clear();
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                SLogger.Log("Couldn't create DB_Tables!", "SDBEngine.cs -> createTables()", ex, LogType.Critical);
            }

        }
        #endregion

        #region DB Functions
        public static async ValueTask InsertAsync(List<SDBValue> values, SDBTable table)
        {
            try
            {
                checkColumns(table, values);

                NpgsqlCommand cmd = new();
                List<string> columnList = new();
                List<string> valueList = new();

                for (int i = 0; i < values.Count; i++)
                {
                    var value = values[i];
                    string columnName = getColumnString(value.Column);
                    string paramName = $"@v{i}";

                    columnList.Add(columnName);
                    valueList.Add(paramName);
                    cmd.Parameters.AddWithValue(paramName, value.Value);
                }

                cmd.CommandText = $"INSERT INTO {SConfig.Database.Schema}.{getTableString(table)} ({string.Join(", ", columnList)}) VALUES ({string.Join(", ", valueList)})";
                cmd.Connection = _Connection;

                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                SLogger.Log("Couldn't insert value in database", "SDBEngine.cs -> InsertAsync()", ex, LogType.Error);
                throw new Exception("Database Error - Check Console for more information!");
            }
        }
        public static async ValueTask ModifyAsync(List<SDBValue> values, SDBTable table, List<SDBValue> conditions)
        {
            try
            {
                checkColumns(table, values);
                checkColumns(table, conditions);

                NpgsqlCommand cmd = new();

                List<string> valuesList = new();
                for (int i = 0; i < values.Count; i++)
                {
                    var value = values[i];
                    string columnName = getColumnString(value.Column);
                    string paramName = $"@v{i}";

                    valuesList.Add($"{columnName} = {paramName}");
                    cmd.Parameters.AddWithValue(paramName, value.Value);
                }

                List<string> conditionsList = new();
                for (int i = 0; i < conditions.Count; i++)
                {
                    var condition = conditions[i];
                    string columnName = getColumnString(condition.Column);
                    string paramName = $"@c{i}";

                    conditionsList.Add($"{columnName} = {paramName}");
                    cmd.Parameters.AddWithValue(paramName, condition.Value);
                }

                cmd.CommandText = $"UPDATE {SConfig.Database.Schema}.{getTableString(table)} SET {string.Join(", ", valuesList)} WHERE {string.Join(" AND ", conditionsList)}";
                cmd.Connection = _Connection;

                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                SLogger.Log("Couldn't modify value in database", "SDBEngine.cs -> ModifyAsync()", ex, LogType.Error);
                throw new Exception("Database Error - Check Console for more information!");
            }
        }
        public static async ValueTask DeleteAsync(SDBTable table, List<SDBValue> conditions)
        {
            try
            {
                checkColumns(table, conditions);

                NpgsqlCommand cmd = new();

                List<string> conditionsList = new();
                for (int i = 0; i < conditions.Count; i++)
                {
                    var condition = conditions[i];
                    string columnName = getColumnString(condition.Column);
                    string paramName = $"@c{i}";

                    conditionsList.Add($"{columnName} = {paramName}");
                    cmd.Parameters.AddWithValue(paramName, condition.Value);
                }

                cmd.CommandText = $"DELETE FROM {SConfig.Database.Schema}.{getTableString(table)} WHERE {string.Join(" AND ", conditionsList)}";
                cmd.Connection = _Connection;

                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                SLogger.Log("Couldn't delete value in database", "SDBEngine.cs -> DeleteAsync()", ex, LogType.Error);
                throw new Exception("Database Error - Check Console for more information!");
            }
        }
        public static async ValueTask<List<string>> SelectAsync(SDBTable table, SDBColumn column, List<SDBValue>? conditions = null, int? limit = null)
        {
            try
            {
                if (conditions != null) checkColumns(table, conditions);

                NpgsqlCommand cmd = new();
                List<string> result = new();

                string command = $"SELECT {getColumnString(column)} FROM {SConfig.Database.Schema}.{getTableString(table)} ";

                if (conditions != null)
                {
                    List<string> conditionsList = new();
                    for (int i = 0; i < conditions.Count; i++)
                    {
                        var condition = conditions[i];
                        string columnName = getColumnString(condition.Column);
                        string paramName = $"@c{i}";

                        conditionsList.Add($"{columnName} = {paramName}");
                        cmd.Parameters.AddWithValue(paramName, condition.Value);
                    }
                    command += $"WHERE {string.Join(" AND ", conditionsList)} ";
                }

                if (limit != null) command += $"LIMIT {limit}";

                cmd.CommandText = command;
                cmd.Connection = _Connection;


                SLogger.Log($"Sending db_cmd {cmd.CommandText}", type: LogType.Debug);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString(0));
                    SLogger.Log($"Selected result {reader.GetString(0)}", type: LogType.Debug); 
                }
                SLogger.Log("Reading Complete", type: LogType.Debug);
                await _Connection.CloseAsync();
                await _Connection.OpenAsync();
                return result;
            }
            catch (Exception ex)
            {
                SLogger.Log("Couldn't select value in database", "SDBEngine.cs -> SelectAsync()", ex, LogType.Error);
                throw new Exception("Database Error - Check Console for more information!");
            }
        }
        #endregion
    }
}