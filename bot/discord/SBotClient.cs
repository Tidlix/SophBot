using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using SophBot.bot.conf;
using SophBot.bot.discord.commands;
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

                builder.ConfigureEventHandlers(events =>
                {
                    events.AddEventHandlers<WelcomeEvents>();
                    events.AddEventHandlers<ErrorEvents>();
                    events.AddEventHandlers<ClientEvents>();
                    events.AddEventHandlers<WikiEvents>();
                });

                builder.ConfigureExtraFeatures(features =>
                {
                    features.LogUnknownAuditlogs = false;
                    features.LogUnknownEvents = false;
                });

                builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
                {
                    extension.AddCommands(Assembly.GetExecutingAssembly());

                    TextCommandProcessor textCommandProcessor = new(new()
                    {
                        PrefixResolver = new DefaultPrefixResolver(false, "!").ResolvePrefixAsync,
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
                    UseDefaultCommandErrorHandler = true,
                });


                Client = builder.Build();
                await builder.ConnectAsync();
            }
            catch (Exception ex)
            {
                SLogger.Log("Couldn't create discord client", "SBotClient.cs -> CreateClientAsync()", ex, LogType.Critical);
            }
            
        }
    }
}