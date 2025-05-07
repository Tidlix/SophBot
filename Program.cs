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
        #region Basic Config
        await Config.ReadAsnyc();

        try {
            await TidlixDB.createDB();
        } catch (Exception e) {
            await MessageSystem.sendMessage($"Couldn't connect to Database. Database will not be reachable! - {e.Message}", MessageType.Error());
        }

        DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(Config.Token, DiscordIntents.All);
        builder.ConfigureExtraFeatures(conf => {
            conf.LogUnknownAuditlogs = false;
            conf.LogUnknownEvents = false;
        }); 
        builder.ConfigureLogging(conf => {
            conf.SetMinimumLevel(LogLevel.Error);
        });

        builder.UseInteractivity();
        #endregion

        #region Event Config
        builder.ConfigureEventHandlers(
            b => b
            .AddEventHandlers<BotEvents>()
            .AddEventHandlers<MemberEvents>()
            .AddEventHandlers<ModalEvents>()
            .AddEventHandlers<ButtonEvents>()
            .AddEventHandlers<MessageEvents>()
        );
        #endregion

        #region Command Config
        builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
        {
            extension.AddCheck<RequireBotOwnerCheck>();

            extension.AddCommands([
                typeof(DebugCommands),
                typeof(ModerationCommands),
                typeof(ModerationCommands.warnCommands),
                typeof(RuleCommands),
                typeof(ServerCommands),
                typeof(ServerCommands.customCommands),
                typeof(CustomCommands),
                typeof(RRCommands)
            ]);

            TextCommandProcessor textCommandProcessor = new(new()
            {
                PrefixResolver = new DefaultPrefixResolver(true, "!", "?").ResolvePrefixAsync,
            });
            SlashCommandProcessor slashCommandProcessor = new(new SlashCommandConfiguration()
            {
                NamingPolicy = new LowercaseNamingPolicy()
            });
            extension.AddProcessor(textCommandProcessor);
            extension.AddProcessor(slashCommandProcessor);
        }, new CommandsConfiguration() {
            UseDefaultCommandErrorHandler = false,
        }
        );
        #endregion


        await builder.ConnectAsync();     

        while (true);
    }
}
