using System.Data;
using SophBot.Universal;

namespace SophBot.Discord.Tools
{
    public static class CustomCommandEngine
    {
        public static CustomCommand? getCommand(string command)
        {
            var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Commands, ["command"], [new("command", "=", command)]);
            if (data.Rows.Count == 0) return null;

            return new CustomCommand(command);
        }
        public static void setCommandResponse(string command, string response, bool syncVariables)
        {
            if (getCommand(command) is null)
                DatabaseEngine.InsertData(DatabaseEngine.DBTable.Commands, new Dictionary<string, object>() { 
                    { "command", command }, { "response", response }, { "sync-vars", syncVariables } });
            else
                DatabaseEngine.ModifyData(DatabaseEngine.DBTable.Commands, new Dictionary<string, object>() { 
                    { "response", response }, { "sync-vars", syncVariables } }, [new("command", "=", command)]);
        }
    }
    public class CustomCommand
    {
        public string command;
        public string response;
        public bool syncVariables;

        public CustomCommand(string command)
        {
            this.command = command;
            var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Commands, ["command"], [new("command", "=", command)]);
            DataRow row = data.Rows[0];
            response = (string)row["response"];
            syncVariables = (bool)row["sync-vars"];
        }

        public override string ToString()
        {
            /*
            if syncVariables = true
              replace every (random) variable with same value
            else
              replace every (random) variable with new random value
            */
            return response;
        }
    }
}