using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.Commands.ChoiceProviders;
using SophBot.Objects;

namespace SophBot.Commands.ModCommands {
    public class ServerCommands {
        [Command("modifyconfig"), Description("Bearbeite die Serverconfig dieses Servers"), RequirePermissions(DiscordPermission.Administrator)] 
        public async ValueTask modConfig (SlashCommandContext ctx, DiscordRole? memberRole = null, DiscordChannel? welcomeChannel = null, DiscordChannel? ruleChannel = null, DiscordRole? mentionRole = null) {
            #pragma warning disable CS8602, CS8604, CS8625
            await ctx.DeferResponseAsync();
            
            try
            {
                TDiscordGuild guild = new(ctx.Guild);
                if (welcomeChannel != null) await guild.setChannelAsnyc(TDiscordGuild.Channel.Welcome, welcomeChannel);
                if (ruleChannel != null) await guild.setChannelAsnyc(TDiscordGuild.Channel.Rules, ruleChannel);
                if (memberRole != null) await guild.setRoleAsync(TDiscordGuild.Role.Member, memberRole);
                if (mentionRole != null) await guild.setRoleAsync(TDiscordGuild.Role.Member, mentionRole);

                await ctx.EditResponseAsync("Die Server-Config wurde erfolgreich bearbeitet!");
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(ex.Message);
            }
        }

        [Command("customcommand")]
        public async ValueTask customCommand(SlashCommandContext ctx, [SlashAutoCompleteProvider<CommandChoiceProvider>] string command)
        {
            TDiscordGuild guild = new(ctx.Guild);

            var modal = new DiscordInteractionResponseBuilder();
            modal.WithTitle(command);
            modal.WithCustomId($"commandModal_{command}");
            modal.AddTextInputComponent(new DiscordTextInputComponent(
                    style: DiscordTextInputStyle.Paragraph,
                    label: "Command-Ausgabe: ",
                    value: await guild.getCommandAsnyc(command),
                    customId: $"commandInput_{command}",
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