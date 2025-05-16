using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.UserCommands {
    public class CustomCommands {
        [Command("ListCommands"), Description("Erhalte eine Liste aller Custom Commands")]
        public async ValueTask listCommands(CommandContext ctx) {
            #pragma warning disable CS8602
            await ctx.DeferResponseAsync();

            try {
                string commandList = "";
                foreach(var command in await TidlixDB.CustomCommands.getAllCommandsAsnyc(ctx.Guild.Id))
                {
                    commandList+= $"- {command} \n";
                }

                DiscordComponent[] components =
                {
                    new DiscordTextDisplayComponent("### Hier ist eine Liste aller Custom-Commands: "),
                    new DiscordTextDisplayComponent(commandList)
                };
                DiscordContainerComponent container = new DiscordContainerComponent(components, color: DiscordColor.Gray);

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddRawComponents(container));
            } catch (Exception ex) {
                await ctx.EditResponseAsync("Fehler - Commands konnten nicht gelesen werden! Bitte kontaktiere den Entwickler dieses Bots!");
                await Log.sendMessage($"Custom command list couldn't be readed! - {ex.Message}", MessageType.Error());
            }
        }
    }
}