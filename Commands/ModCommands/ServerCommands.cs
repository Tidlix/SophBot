using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.Commands.ChoiceProviders;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.ModCommands {
    public class ServerCommands {
        [Command("modifyconfig"), Description("Bearbeite die Serverconfig dieses Servers"), RequirePermissions(DiscordPermission.Administrator)] 
        public async ValueTask modConfig (SlashCommandContext ctx, DiscordRole? memberRole = null, DiscordChannel? welcomeChannel = null, DiscordChannel? ruleChannel = null, DiscordRole? mentionRole = null) {
            #pragma warning disable CS8602, CS8604, CS8625
            await ctx.DeferResponseAsync();

            try {
                if (memberRole != null ) await TidlixDB.ServerConfig.modifyValueAsnyc("memberrole", memberRole.Id.ToString(), ctx.Guild.Id);
                if (welcomeChannel != null ) await TidlixDB.ServerConfig.modifyValueAsnyc("welcomechannel", welcomeChannel.Id.ToString(), ctx.Guild.Id);
                if (ruleChannel != null ) await TidlixDB.ServerConfig.modifyValueAsnyc("rulechannel", ruleChannel.Id.ToString(), ctx.Guild.Id);
                if (mentionRole != null ) await TidlixDB.ServerConfig.modifyValueAsnyc("mentionrole", mentionRole.Id.ToString(), ctx.Guild.Id); 

                await ctx.EditResponseAsync("Die Server-Config wurde erfolgreich bearbeitet!");                
            } catch (Exception ex) {
                await ctx.EditResponseAsync("Fehler - Server Konfiguration konnte nicht bearbeitet werden! Bitte kontaktiere den Entwickler dieses Bots!");
                await Log.sendMessage($"Serverconfig for server {ctx.Guild.Name}({ctx.Guild.Id}) couldn't be updated! - {ex.Message}", MessageType.Error());
            }
        }

        [Command("customcommand")]
        public async ValueTask customCommand(SlashCommandContext ctx, [SlashAutoCompleteProvider<CommandChoiceProvider>] string command)
        {
            var modal = new DiscordInteractionResponseBuilder();
            modal.WithTitle(command);
            modal.WithCustomId($"commandModal_{command}");
            modal.AddComponents(new DiscordTextInputComponent(
                    style: DiscordTextInputStyle.Paragraph,
                    label: "Command-Ausgabe: ",
                    value: await TidlixDB.CustomCommands.getCommandAsnyc(command, ctx.Guild.Id),
                    customId: $"commandInput_{command}",
                    required: false,
                    max_length: 2000
                ));
            modal.AddComponents(new DiscordTextInputComponent(
                        label: "Verfügbare Variablen: ",
                        customId: "variables",
                        required: false,
                        style: DiscordTextInputStyle.Paragraph,
                        value: "[rand(max)] - Zufallszahl (max -> höchster Wert)\n[rand(min, max)] - Zufallszahl (min -> kleinster Wert; max -> höchster Wert) \n[user] - Ausführender Nutzer\n[text] - Gesamter Text hinter dem Command\n[word(index)] - Bestimmtest Wort des Textes nach dem Command (z.B. [word(2)] -> zweites Wort nach Command)"));

            await ctx.RespondWithModalAsync(modal);
        }
    }
}