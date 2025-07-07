using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.conf;
using SophBot.bot.discord.events;
using SophBot.bot.logs;

namespace SophBot.bot.discord
{
    public class SBotClient
    {
#pragma warning disable CS8618
        public static DiscordClient Client;
#pragma warning disable CS8618

        public static async ValueTask CreateClientAsync()
        {
            try
            {
                DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(SConfig.Discord.Token, DiscordIntents.All);
                builder.DisableDefaultLogging();
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new SLogger.LoggerProvider());
                    logging.SetMinimumLevel(LogLevel.Trace);
                });

                builder.ConfigureEventHandlers(events =>
                {
                    events.AddEventHandlers<ErrorEvents>();
                    events.AddEventHandlers<ClientEvents>();
                    events.AddEventHandlers<LogEvents>();
                    events.AddEventHandlers<WelcomeEvents>();
                    events.AddEventHandlers<CCEvents>();
                    events.AddEventHandlers<ProfileEvents>();
                    events.AddEventHandlers<GambleEvents>();
                    events.AddEventHandlers<WikiEvents>();
                    events.AddEventHandlers<RREvents>();
                    events.AddEventHandlers<RuleEvents>();
                });

                builder.ConfigureExtraFeatures(features =>
                {
                    features.LogUnknownAuditlogs = false;
                    features.LogUnknownEvents = false;
                });

                builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
                {
                    extension.AddCommands(Assembly.GetExecutingAssembly());
                    extension.CommandErrored += async (s, e) =>
                    {
                        SLogger.Log(LogLevel.Error, $"A Command failed", $"{e.CommandObject}", e.Exception);

                        try { await e.Context.RespondAsync("Ein Fehler ist aufgetreten!"); }
                        catch { await e.Context.FollowupAsync("Ein Fehler ist aufgetreten!"); }
                    };

                    TextCommandProcessor textCommandProcessor = new(new()
                    {
                        PrefixResolver = new DefaultPrefixResolver(false, "?").ResolvePrefixAsync,
                    });
                    SlashCommandProcessor slashCommandProcessor = new(new SlashCommandConfiguration()
                    {
                        NamingPolicy = new LowercaseNamingPolicy()
                    });

                    extension.AddProcessor(textCommandProcessor);
                    extension.AddProcessor(slashCommandProcessor);
                },
                new CommandsConfiguration()
                {
                    UseDefaultCommandErrorHandler = false,
                });


                Client = builder.Build();
                await builder.ConnectAsync();
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Critical, "Couldn't create discord client", "SBotClient.cs", exception: ex);
            }
            
        }
    }
}