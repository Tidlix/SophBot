using System.Data;
using System.Text.RegularExpressions;
using SophBot.Universal;

namespace SophBot.Discord.Tools
{
    public static class CustomCommandEngine
    {
        public static CustomCommand? getCommand(string command)
        {
            var data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Commands, ["command"], [new("command", "=", command)]);
            if (data.Rows.Count == 0) return null;

            return new CustomCommand(command);
        }
        public static void setCommandResponse(string command, string response, bool syncVariables)
        {
            if (getCommand(command) is null)
                Program.GetService<DatabaseService>().InsertData(DBTable.Commands, new Dictionary<string, object>() { 
                    { "command", command }, { "response", response }, { "sync-vars", syncVariables } });
            else
                Program.GetService<DatabaseService>().ModifyData(DBTable.Commands, new Dictionary<string, object>() { 
                    { "response", response }, { "sync-vars", syncVariables } }, [new("command", "=", command)]);
        }
        public static void deleteCommand(string command)
        {
            Program.GetService<DatabaseService>().DeleteData(DBTable.Commands, [new("command", "=", command)]);
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
            var data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Commands, ["response", "sync-vars"], [new("command", "=", command)]);
            DataRow row = data.Rows[0];
            response = (string)row["response"];
            syncVariables = (bool)row["sync-vars"];
        }

        public string ToString(string[] input, string senderMention)
        {
            string text = response;
            string inputPattern = @"{input\((?<id>[0-9])\)}|{input}";
            string randPattern = @"{rand\((?<max>[0-9]+)\)}|{rand\((?<min>[0-9]+),(?<max>[0-9]+)\)}";
            string errorPattern = @"\[ERROR\((?<id>[0-9]+)\)\]";


            Random rand = new ();
            Dictionary<string, int> syncCache = new();

            text = text.Replace("{sender}", $"{senderMention}");

            text = Regex.Replace(text, inputPattern, match =>
            {
                if (string.IsNullOrEmpty(match.Groups["id"].Value)) 
                    return string.Join(' ', input);
                else
                {
                    if(int.TryParse(match.Groups["id"].Value, out int index))
                    {
                        if (input.Count() > index)
                            return input[index];
                        else
                            return "[ERROR(10)]";
                    }
                    else return "[ERROR(11)]";
                }
            });

            text = Regex.Replace(text, randPattern, match =>
            {
                if(string.IsNullOrEmpty(match.Groups["min"].Value))
                {
                    string maxValue = match.Groups["max"].Value;
                    string cacheKey = $"max:{maxValue}";
                    if(syncVariables && syncCache.TryGetValue(cacheKey, out int cacheValue))
                    {
                        return cacheValue.ToString();
                    }
                    if (int.TryParse(maxValue, out int x))
                    {
                        int randomNr = rand.Next(x+1);
                        if (syncVariables) syncCache.Add(cacheKey, randomNr);
                        return randomNr.ToString();
                    }
                    else return "[ERROR(11)]";
                }
                else
                {
                    string minValue = match.Groups["min"].Value;
                    string maxValue = match.Groups["max"].Value;
                    string cacheKey = $"min:{minValue};max:{maxValue}";
                    if(syncVariables && syncCache.TryGetValue(cacheKey, out int cacheValue))
                    {
                        return cacheValue.ToString();
                    }
                    if (int.TryParse(minValue, out int x) && int.TryParse(maxValue, out int y))
                    {
                        int randomNr = rand.Next(x, y+1);
                        if (syncVariables) syncCache.Add(cacheKey, randomNr);
                        return randomNr.ToString();
                    }
                    else return "[ERROR(11)]";
                }
            });

            MatchCollection errorMatches = Regex.Matches(text, errorPattern);
            if(errorMatches.Count > 0)
            {
                text = "Bei der Konvertierung des Commands ist mindestens ein Fehler Aufgetreten!";
                foreach(Match error in errorMatches)
                {
                    text += $"\n> -# Fehlercode: {error.Groups["id"].Value} - Beschreibung: {error.Groups["id"].Value switch
                    {
                        "10" => "Unvolständige Nutzereingabe!",
                        "11" => "Unerwartete Variable!",
                        _ => "Unbekannter Fehler!"
                    }} - an Position: {error.Index}";
                }
            }
            return text;
        }
    }
}