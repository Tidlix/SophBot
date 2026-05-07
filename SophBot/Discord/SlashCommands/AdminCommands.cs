using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using SophBot.Discord.Tools;
using SophBot.Discord.Tools.OptionProviders;
using SophBot.Universal;

namespace SophBot.Discord.SlashCommands
{
    public class AdminCommands
    {
        [Command("modify-cc"), RequirePermissions(DiscordPermission.ManageChannels)]
        public async Task modifyCustomCommand(SlashCommandContext ctx, [SlashAutoCompleteProvider<CustomCommandOptionProvider>] string command)
        {
            CustomCommand? current = CustomCommandEngine.getCommand(command);
            command = command.ToLower();
            if(command.Contains(' '))
            {
                await ctx.RespondAsync("Ein Command darf keine Leerzeichen enthalten!", true);
                return;
            }

            var modal = new DiscordModalBuilder()
                .WithTitle($"Custom Command - !{command}")
                .WithCustomId($"commandModify{command}")
                .AddTextInput(
                    new("commandModifyInput", "Command-Output hier einfügen!", (current is null) ? null : current.response, false, DiscordTextInputStyle.Paragraph),
                    "Command Ausgabe:")
                .AddCheckbox(new DiscordCheckboxComponent("commandModifySyncbox", (current is null) ? false : current.syncVariables), "Variablen Synchronisieren")
                .AddTextDisplay(@"Folgende Variablen stehen zur Auswahl:
-# {rand(x)} -> Zufällige Zahl zwischen 0 und x
-# {rand(x,y)} -> Zufällige Zahl zwischen x und y
-# {sender} -> Sender der Nachricht
-# {input} -> Text welcher hinter dem Command geschrieben wurde
-# {input(x)} -> x. Wort des Textes welcher hinter dem Command geschrieben wurde");
            await ctx.RespondWithModalAsync(modal);
            var response = await Program.GetService<InteractivityExtension>().WaitForModalAsync($"commandModify{command}", TimeSpan.FromMinutes(30));
            
            await response.Result.Interaction.DeferAsync(true);
            if (response.TimedOut)
            {
                await response.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Die Antwort hat zu lange gedauert!"));
                return;
            }

            var cmdResponse = response.Result.Values[$"commandModifyInput"] as TextInputModalSubmission;
            var syncVariables = response.Result.Values[$"commandModifySyncbox"] as CheckboxModalSubmission;

            if(string.IsNullOrEmpty(cmdResponse!.Value))
            {
                CustomCommandEngine.deleteCommand(command);
                await response.Result.Interaction.EditOriginalResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Der Command `!{command}` wurde gelöscht!"));
                //.AddLog($"CC was deleted by {ctx.User}!");
                return;
            }
            CustomCommandEngine.setCommandResponse(command, cmdResponse!.Value, syncVariables!.Value);

            await response.Result.Interaction.EditOriginalResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Der Command `!{command}` wurde bearbeitet!"));
            //Logs.AddLog($"CC was modified by {ctx.User}! - New content: {cmdResponse.Value}", Microsoft.Extensions.Logging.LogLevel.Information, "SophBot.AdminCommands");
        }
    }
}