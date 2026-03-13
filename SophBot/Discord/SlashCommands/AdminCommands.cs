using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.Discord.Tools;
using SophBot.Discord.Tools.OptionProviders;

namespace SophBot.Discord.SlashCommands
{
    public class AdminCommands
    {
        [Command("modify-cc")]
        public async Task modifyCustomCommand(SlashCommandContext ctx, [SlashAutoCompleteProvider<CustomCommandOptionProvider>] string command)
        {
            CustomCommand? current = CustomCommandEngine.getCommand(command);

            var modal = new DiscordModalBuilder()
                .WithTitle($"Custom Command - !{command}")
                .WithCustomId($"commandModify{command}")
                .AddTextInput(
                    new("commandModifyInput", "Command-Output hier einfügen!", (current is null) ? null : current.response, true, DiscordTextInputStyle.Paragraph),
                    "Command Ausgabe:")
                .AddCheckbox(new DiscordCheckboxComponent("commandModifySyncbox", true), "Synchronisierte Variablen:")
                .AddTextDisplay(@"Folgende Variablen stehen zur Auswahl:
-# {rand(x)} -> Zufällige Zahl zwischen 0 und x
-# {rand(x,y)} -> Zufällige Zahl zwischen x und y
-# {sender} -> Sender der Nachricht
-# {text} -> Text welcher hinter dem Command geschrieben wurde
-# {text(x)} -> x. Wort des Textes welcher hinter dem Command geschrieben wurde")
                .AddTextDisplay("WARNING ");
            await ctx.RespondWithModalAsync(modal);
        }
    }
}