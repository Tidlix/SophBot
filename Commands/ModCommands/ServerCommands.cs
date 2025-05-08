using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.ModCommands {
    public class ServerCommands {
        [Command("modifyconfig"), Description("Bearbeite die Serverconfig dieses Servers"), RequirePermissions(DiscordPermission.Administrator)] 
        public async Task modConfig (SlashCommandContext ctx, DiscordRole? memberRole = null, DiscordChannel? welcomeChannel = null, DiscordChannel? ruleChannel = null, DiscordRole? mentionRole = null) {
            #pragma warning disable CS8602, CS8604, CS8625
            await ctx.DeferResponseAsync();

            try {
                if (memberRole != null ) await TidlixDB.modifyServerconfig("memberrole", memberRole.Id, ctx.Guild.Id);
                if (welcomeChannel != null ) await TidlixDB.modifyServerconfig("welcomechannel", welcomeChannel.Id, ctx.Guild.Id);
                if (ruleChannel != null ) await TidlixDB.modifyServerconfig("rulechannel", ruleChannel.Id, ctx.Guild.Id);
                if (mentionRole != null ) await TidlixDB.modifyServerconfig("mentionrole", mentionRole.Id, ctx.Guild.Id); 

                await ctx.EditResponseAsync("Die Server-Config wurde erfolgreich bearbeitet!");                
            } catch (Exception ex) {
                await ctx.EditResponseAsync("Fehler - Server Konfiguration konnte nicht bearbeitet werden! Bitte kontaktiere den Entwickler dieses Bots!");
                await MessageSystem.sendMessage($"Serverconfig for server {ctx.Guild.Name}({ctx.Guild.Id}) couldn't be updated! - {ex.Message}", MessageType.Error());
            }
        }


        [Command ("CustomCommand"), RequirePermissions(DiscordPermission.Administrator)]
        public class customCommands {
            [Command("Create"), Description("Erstelle einen Command")]
            public async Task createCommand(SlashCommandContext ctx, [Description("Der Command den du erstellen möchtest")]string command) {
                try {
                    command = command.ToLower();
                    if (await TidlixDB.checkCommandExists(command, ctx.Guild.Id)) {
                        await ctx.RespondAsync("Fehler - Dieser Command exestiert bereits!");
                        return;
                    }
                    if (command.Contains(" ")) {
                        await ctx.RespondAsync("Fehler - Commands dürfen kein Leerzeichen enthalten!");
                        return;
                    }

                    var modal = new DiscordInteractionResponseBuilder()
                    {
                        Title = $"Creating \"!{command}\"",
                        CustomId = $"createCommand{command}",
                    };
                    modal.AddComponents(new DiscordTextInputComponent(label: $"Was soll der Command ausgeben:", customId: $"createCommandOutput{command}", max_length: 2000, style: DiscordTextInputStyle.Paragraph));
                    modal.AddComponents(new DiscordTextInputComponent(
                        label: "Verfügbare Variablen: ", 
                        customId: "variables", 
                        required: false, 
                        style: DiscordTextInputStyle.Paragraph,
                        value: "[rand(max)] - Zufallszahl (max -> höchster Wert)\n[rand(min, max)] - Zufallszahl (min -> kleinster Wert; max -> höchster Wert) \n[user] - Ausführender Nutzer\n[text] - Gesamter Text hinter dem Command\n[word(index)] - Bestimmtest Wort des Textes nach dem Command (z.B. [word(2)] -> zweites Wort nach Command)"));

                    await ctx.RespondWithModalAsync(modal);

                } catch (Exception ex) {
                    await ctx.Channel.SendMessageAsync("Fehler - Command konnte nicht erstellt werden! Bitte kontaktiere den Entwickler dieses Bots!");
                    await MessageSystem.sendMessage($"Command for server {ctx.Guild.Name}({ctx.Guild.Id}) couldn't be created! - {ex.Message}", MessageType.Error());
                }
                
            }

            [Command("Delete"), Description("Lösche einen Command")]
            public async Task deleteCommand(SlashCommandContext ctx, [Description("Der Command den du löschen möchtest")]string command) {
                await ctx.DeferResponseAsync();
                command = command.ToLower();
                try {
                    if (!await TidlixDB.checkCommandExists(command, ctx.Guild.Id)) {
                        await ctx.EditResponseAsync("Fehler - Dieser Command exestiert nicht!");
                        return;
                    }

                    await TidlixDB.deleteCommand(command, ctx.Guild.Id);
                    await ctx.EditResponseAsync("Der Command wurde erfolgreich gelöscht!");
                } catch (Exception ex) {
                    await ctx.RespondAsync("Fehler - Command konnte nicht gelöscht werden! Bitte kontaktiere den Entwickler dieses Bots!");
                    await MessageSystem.sendMessage($"Command for server {ctx.Guild.Name}({ctx.Guild.Id}) couldn't be deleted! - {ex.Message}", MessageType.Error());
                }
            }

            [Command("Modify"), Description("Bearbeite einen Command")]
            public async Task modifyCommand(SlashCommandContext ctx, [Description("Der Command den du bearbeiten möchtest")]string command) {
                try {
                    command = command.ToLower();
                    if (!await TidlixDB.checkCommandExists(command, ctx.Guild.Id)) {
                        await ctx.RespondAsync("Fehler - Dieser Command exestiert nicht!");
                        return;
                    }

                    var modal = new DiscordInteractionResponseBuilder()
                    {
                        Title = $"Modifying \"!{command}\"",
                        CustomId = $"modifyCommand{command}",
                    };
                    
                    modal.AddComponents(new DiscordTextInputComponent(label: $"Was soll der Command ausgeben:", customId: $"createCommandOutput{command}", value: await TidlixDB.getCommand(command, ctx.Guild.Id), max_length: 2000, style: DiscordTextInputStyle.Paragraph));
                    modal.AddComponents(new DiscordTextInputComponent(
                        label: "Verfügbare Variablen: ", 
                        customId: "variables", 
                        required: false, 
                        style: DiscordTextInputStyle.Paragraph,
                        value: "[rand(max)] - Zufallszahl (max -> höchster Wert)\n[rand(min, max)] - Zufallszahl (min -> kleinster Wert; max -> höchster Wert) \n[user] - Ausführender Nutzer\n[text] - Gesamter Text hinter dem Command\n[word(index)] - Bestimmtest Wort des Textes nach dem Command (z.B. [word(2)] -> zweites Wort nach Command)"));
                    await ctx.RespondWithModalAsync(modal);

                } catch (Exception ex) {
                    await ctx.RespondAsync("Fehler - Command konnte nicht bearbeitet werden! Bitte kontaktiere den Entwickler dieses Bots!");
                    await MessageSystem.sendMessage($"Command for server {ctx.Guild.Name}({ctx.Guild.Id}) couldn't be modified! - {ex.Message}", MessageType.Error());
                }
            }
        }
    }
}