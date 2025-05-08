using CommunityToolkit.HighPerformance.Helpers;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SophBot.Commands.ContextChecks;
using SophBot.Commands.ModCommands;
using SophBot.Commands.UsercCommands;
using SophBot.Configuration;
using SophBot.Database;
using SophBot.EventHandlers;
using SophBot.Messages;
namespace SophBot;

class Program
{
    static async Task Main(string[] args)
    {
        await Config.ReadAsnyc();

        try {
            await TidlixDB.createDB();
        } catch (Exception e) {
            await MessageSystem.sendMessage($"Couldn't connect to Database. Database will not be reachable! - {e.Message}", MessageType.Error());
        }
        await Clients.CreateDiscordClient(); 
        await Clients.CreateTwitchMonitoring();  

        while (true);
    }
}
