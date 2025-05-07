using System.ComponentModel;
using DSharpPlus.Commands;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.Commands.UsercCommands {
    public class CustomCommands {
        [Command("ListCommands"), Description("Erhalte eine Liste aller Custom Commands")]
        public async ValueTask listCommands(CommandContext ctx) {
            #pragma warning disable CS8602
            await ctx.DeferResponseAsync();

            try {
                await ctx.EditResponseAsync(await TidlixDB.getAllCommands(ctx.Guild.Id));
            } catch (Exception ex) {
                await ctx.EditResponseAsync("Fehler - Commands konnten nicht gelesen werden! Bitte kontaktiere den Entwickler dieses Bots!");
                await MessageSystem.sendMessage($"Custom command list couldn't be readed! - {ex.Message}", MessageType.Error());
            }
        }
    }
}