using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using SophBot.bot.conf;
using SophBot.bot.logs;

namespace SophBot.bot.database
{
    public class SDBEngine
    {
        public static string ConnString = $"Host={SConfig.Database.Host}:{SConfig.Database.Port};Database ={SConfig.Database.DB_Name};Username={SConfig.Database.Username};Password={SConfig.Database.Password};";

        #region Commands

        private static object ConvertToDbValue(object? value)
        {
            if (value is null) return DBNull.Value!; // suppress CS8603

            return value switch
            {
                ulong ul => Convert.ToInt64(ul),
                uint ui => Convert.ToInt32(ui),
                ushort us => Convert.ToInt32(us),
                short s => s,
                byte b => b,
                sbyte sb => sb,
                char c => c.ToString(),
                bool b => b,
                Enum e => Convert.ToInt32(e),
                DateTime dt => dt,
                float f => f,
                double d => d,
                decimal dec => dec,
                string str => str,
                Guid g => g,
                _ => value.ToString()!
            };
        }

        private static void AddParameters(NpgsqlCommand cmd, Dictionary<string, object> dict, string prefix = "p")
        {
            int index = 0;
            foreach (var kvp in dict)
            {
                string paramName = $"@{prefix}{index}";
                cmd.Parameters.AddWithValue(paramName, ConvertToDbValue(kvp.Value));
                index++;
            }
        }

        public static async Task<object[][]> SelectFromAsync(
            string table,
            string[] columns,
            Dictionary<string, object>? conditions = null,
            string? orderBy = null,
            bool desc = false,
            int? limit = null)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnString);
                await conn.OpenAsync();

                // Spaltentypen abrufen, um Parameter korrekt zu setzen
                var columnTypes = new Dictionary<string, NpgsqlDbType>();
                using (var typeCmd = new NpgsqlCommand(
                    $"SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table",
                    conn))
                {
                    typeCmd.Parameters.AddWithValue("@schema", SConfig.Database.Schema);
                    typeCmd.Parameters.AddWithValue("@table", table);

                    using var reader = await typeCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string colName = reader.GetString(0);
                        string dataType = reader.GetString(1);

                        columnTypes[colName] = dataType switch
                        {
                            "integer" => NpgsqlDbType.Integer,
                            "bigint" => NpgsqlDbType.Bigint,
                            "smallint" => NpgsqlDbType.Smallint,
                            "text" => NpgsqlDbType.Text,
                            "character varying" => NpgsqlDbType.Varchar,
                            "boolean" => NpgsqlDbType.Boolean,
                            "uuid" => NpgsqlDbType.Uuid,
                            "timestamp without time zone" => NpgsqlDbType.Timestamp,
                            "timestamp with time zone" => NpgsqlDbType.TimestampTz,
                            "numeric" => NpgsqlDbType.Numeric,
                            _ => NpgsqlDbType.Text
                        };
                    }
                }

                using var cmd = new NpgsqlCommand();
                string quotedCols = string.Join(", ", columns.Select(c => $"\"{c}\""));
                string command = $"SELECT {quotedCols} FROM \"{SConfig.Database.Schema}\".\"{table}\"";

                if (conditions != null && conditions.Count > 0)
                {
                    var whereClauses = new List<string>();
                    int paramIndex = 0;

                    foreach (var kvp in conditions)
                    {
                        string paramName = $"@p{paramIndex}";
                        whereClauses.Add($"\"{kvp.Key}\" = {paramName}");

                        if (columnTypes.TryGetValue(kvp.Key, out var npgType))
                        {
                            object value = kvp.Value ?? DBNull.Value;

                            value = npgType switch
                            {
                                NpgsqlDbType.Integer => Convert.ToInt32(value),
                                NpgsqlDbType.Bigint => Convert.ToInt64(value),
                                NpgsqlDbType.Smallint => Convert.ToInt16(value),
                                NpgsqlDbType.Boolean => Convert.ToBoolean(value),
                                NpgsqlDbType.Numeric => Convert.ToDecimal(value),
                                NpgsqlDbType.Timestamp => Convert.ToDateTime(value),
                                NpgsqlDbType.TimestampTz => Convert.ToDateTime(value),
                                NpgsqlDbType.Uuid => Guid.Parse(value.ToString()!),
                                _ => value
                            };

                            cmd.Parameters.Add(new NpgsqlParameter(paramName, npgType) { Value = value });
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(paramName, ConvertToDbValue(kvp.Value));
                        }

                        paramIndex++;
                    }


                    command += $" WHERE {string.Join(" AND ", whereClauses)}";
                }

                if (!string.IsNullOrEmpty(orderBy))
                    command += $" ORDER BY \"{orderBy}\" {(desc ? "DESC" : "ASC")}";

                if (limit.HasValue && limit.Value > 0)
                    command += $" LIMIT {limit.Value}";

                cmd.CommandText = command;
                cmd.Connection = conn;

                SLogger.Log(LogLevel.Debug,
                    $"Executing SQL: {command} with params: {string.Join(", ", cmd.Parameters.Cast<NpgsqlParameter>().Select(p => p.ParameterName + '=' + p.Value))}",
                    "SDBEngine.cs");

                using var resultReader = await cmd.ExecuteReaderAsync();
                var results = new List<object[]>();

                while (await resultReader.ReadAsync())
                {
                    var row = new object[resultReader.FieldCount];
                    resultReader.GetValues(row);
                    results.Add(row);
                }

                return results.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to select DB Values - {ex.Message}", ex);
            }
        }

        public static async Task<int> InsertToAsync(string table, Dictionary<string, object>[] rows, bool? distinct = false)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnString);
                using var cmd = new NpgsqlCommand();

                if (rows.Length == 0) return 0;

                var columns = string.Join(", ", rows[0].Keys.Select(k => $"\"{k}\""));
                var valueGroups = new List<string>();
                int globalIndex = 0;

                foreach (var row in rows)
                {
                    var paramNames = new List<string>();
                    foreach (var kvp in row)
                    {
                        string paramName = $"@p{globalIndex}";
                        paramNames.Add(paramName);
                        cmd.Parameters.AddWithValue(paramName, ConvertToDbValue(kvp.Value));
                        globalIndex++;
                    }
                    valueGroups.Add($"({string.Join(", ", paramNames)})");
                }

                string command = $"INSERT {(distinct == true ? "DISTINCT" : string.Empty)} INTO \"{SConfig.Database.Schema}\".\"{table}\" ({columns}) VALUES {string.Join(", ", valueGroups)}";

                cmd.CommandText = command;
                cmd.Connection = conn;

                SLogger.Log(LogLevel.Debug,
                    $"Executing SQL: {command} with params: {string.Join(", ", cmd.Parameters.Cast<NpgsqlParameter>().Select(p => p.ParameterName + '=' + p.Value))}",
                    "SDBEngine.cs");

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert DB Values - {ex.Message}", ex);
            }
        }

        public static async Task<int> DeleteFromAsync(string table, Dictionary<string, object> conditions)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnString);
                using var cmd = new NpgsqlCommand();

                AddParameters(cmd, conditions);

                string command = $"DELETE FROM \"{SConfig.Database.Schema}\".\"{table}\" WHERE {string.Join(" AND ", conditions.Keys.Select((k, i) => $"\"{k}\" = @p{i}"))}";

                cmd.CommandText = command;
                cmd.Connection = conn;

                SLogger.Log(LogLevel.Debug,
                    $"Executing SQL: {command} with params: {string.Join(", ", cmd.Parameters.Cast<NpgsqlParameter>().Select(p => p.ParameterName + '=' + p.Value))}",
                    "SDBEngine.cs");

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete DB Values - {ex.Message}", ex);
            }
        }

        public static async Task<int> ModifyAtAsync(string table, Dictionary<string, object> values, Dictionary<string, object> conditions)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnString);
                using var cmd = new NpgsqlCommand();

                AddParameters(cmd, values, "v");
                AddParameters(cmd, conditions, "c");

                string command = $"UPDATE \"{SConfig.Database.Schema}\".\"{table}\" SET {string.Join(", ", values.Keys.Select((k, i) => $"\"{k}\" = @v{i}"))} WHERE {string.Join(" AND ", conditions.Keys.Select((k, i) => $"\"{k}\" = @c{i}"))}";

                cmd.CommandText = command;
                cmd.Connection = conn;

                SLogger.Log(LogLevel.Debug,
                    $"Executing SQL: {command} with params: {string.Join(", ", cmd.Parameters.Cast<NpgsqlParameter>().Select(p => p.ParameterName + '=' + p.Value))}",
                    "SDBEngine.cs");

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update DB Values - {ex.Message}", ex);
            }
        }

        #endregion
    }
}
