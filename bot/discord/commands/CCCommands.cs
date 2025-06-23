using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.bot.discord.commands.choiceProviders;

namespace SophBot.bot.discord.commands
{
    public class CCommandCommands
    {
        [Command("customcommand"), Description("Erstelle/Bearbeite ein Custom Command"), RequirePermissions(DSharpPlus.Entities.DiscordPermission.Administrator)]
        public async ValueTask customCommand(SlashCommandContext ctx, [SlashAutoCompleteProvider<CCProvider>] string command)
        {
            SDiscordServer server = new(ctx.Guild!);

            command = command.ToLower();

            var modal = new DiscordInteractionResponseBuilder();
            modal.WithTitle(command);
            modal.WithCustomId($"ccSystem_cmd={command};");
            modal.AddTextInputComponent(new DiscordTextInputComponent(
                    style: DiscordTextInputStyle.Paragraph,
                    label: "Command-Ausgabe: ",
                    value: await server.Commands.getOutputAsync(command),
                    customId: $"ccSytem-Input_cmd={command};",
                    required: false,
                    max_length: 2000
                ));
            modal.AddTextInputComponent(new DiscordTextInputComponent(
                        label: "Verfügbare Variablen: ",
                        customId: "variables",
                        required: false,
                        style: DiscordTextInputStyle.Paragraph,
                        value: "[rand(max)] - Zufallszahl (max -> höchster Wert)\n[rand(min, max)] - Zufallszahl (min -> kleinster Wert; max -> höchster Wert) \n[user] - Ausführender Nutzer\n[text] - Gesamter Text hinter dem Command\n[word(index)] - Bestimmtest Wort des Textes nach dem Command (z.B. [word(2)] -> zweites Wort nach Command)"));

            await ctx.RespondWithModalAsync(modal);           
        }
    }
}