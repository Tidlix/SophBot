using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.Database;

namespace SophBot.Commands.ChoiceProviders {
    public class CommandChoiceProvider : IAutoCompleteProvider
    {
        public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx)
        {
            #pragma warning disable CS8602
            var commands = await TidlixDB.getAllCommands(ctx.Guild.Id);

            var list = new List<DiscordAutoCompleteChoice>();

            foreach (var command in commands)
            {
                list.Add(new DiscordAutoCompleteChoice(command, command));
            }

            return await new ValueTask<IEnumerable<DiscordAutoCompleteChoice>>(list);        
        }
    }
}