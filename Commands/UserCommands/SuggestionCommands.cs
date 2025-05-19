using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SophBot.Commands.UserCommands
{
    public class SuggestionCommands
    {
        [Command("Suggestion"), Description("Schlage ein Feature für diesen Bot vor")]
        public static async ValueTask suggestionCommand(SlashCommandContext ctx)
        {
            var modal = new DiscordInteractionResponseBuilder()
            .WithCustomId("suggestionModal")
            .WithTitle("Suggestion")
            .AddComponents(new DiscordTextInputComponent("Deine Idee:", "suggestionModalInput", placeholder: "Bitte beschreibe deine Idee so genau wie möglich", style: DiscordTextInputStyle.Paragraph));

            await ctx.RespondWithModalAsync(modal);
        }
    }
}