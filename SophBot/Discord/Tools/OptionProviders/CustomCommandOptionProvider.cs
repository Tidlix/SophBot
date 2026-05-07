using System.Data;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.Universal;

namespace SophBot.Discord.Tools.OptionProviders
{
    public class CustomCommandOptionProvider : IAutoCompleteProvider
    {
        #pragma warning disable CS1998
        public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
        #pragma warning restore CS1998
        {
            var result = new List<DiscordAutoCompleteChoice>();
            var commands = new List<string>();
            var data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Commands, ["command"], [new("command", "LIKE", $"%{context.UserInput ?? ""}%")]);

            foreach(DataRow currentRow in data.Rows)
            {
                string command = (string)currentRow["command"];
                if (!commands.Contains(command))
                {
                    commands.Add(command);
                    result.Add(new(command, command));
                }
            }
            return result; 
        }
    }
}