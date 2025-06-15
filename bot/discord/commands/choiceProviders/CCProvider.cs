using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands.choiceProviders
{
    public class CCProvider : IAutoCompleteProvider
    {
        public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx)
        {
            SDiscordServer server = new(ctx.Guild!);
            var commands = await server.getCCListAsync();

            if (commands == null) return new List<DiscordAutoCompleteChoice>() { new DiscordAutoCompleteChoice("404 Not Found", "") };
            
            var list = new List<DiscordAutoCompleteChoice>();

            foreach (var command in commands)
            {
                list.Add(new DiscordAutoCompleteChoice(command, command));
            }

            return await new ValueTask<IEnumerable<DiscordAutoCompleteChoice>>(list);  
        }
    }
}