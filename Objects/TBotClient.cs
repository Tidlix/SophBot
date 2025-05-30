using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SophBot.Commands.ModCommands;
using SophBot.Commands.UserCommands;
using SophBot.Configuration;
using SophBot.EventHandlers;

namespace SophBot.Objects
{
    public static class TBotClient
    {
        #pragma warning disable CS8618
        public static DiscordClient Client { get; set; }

        public static async ValueTask CreateDiscordClient(LogLevel logLevel)
        {
            #region Create ClientBuilder
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(Config.Discord.Token, DiscordIntents.All);
            builder.ConfigureExtraFeatures(conf =>
            {
                conf.LogUnknownAuditlogs = false;
                conf.LogUnknownEvents = false;
            });

            
            builder.ConfigureLogging(conf =>
            {
                conf.SetMinimumLevel(LogLevel.Debug);             
            });

            builder.UseInteractivity();
            #endregion

            #region ConfigureEvents
            builder.ConfigureEventHandlers(
                b => b
                .AddEventHandlers<BotEvents>()
                .AddEventHandlers<MemberEvents>()
                .AddEventHandlers<ModalEvents>()
                .AddEventHandlers<ButtonEvents>()
                .AddEventHandlers<MessageEvents>()
            );
            #endregion

            #region ConfigureCommands
            builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
            {
                #pragma warning disable DSP1002
                extension.AddCommands([
                    typeof(DebugCommands),
                    typeof(ModerationCommands),
                    typeof(ModerationCommands.WarnCommands),
                    typeof(RuleCommands),
                    typeof(ServerCommands),
                    typeof(ProfileCommands),
                    typeof(ProfileCommands.Gamble),
                    typeof(ProfileCommands.PointCommands),
                    typeof(SuggestionCommands),
                    typeof(CustomCommands),
                    typeof(RRCommands),
                    typeof(TwitchCommands),
                    typeof(AICommands)
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
            },
            new CommandsConfiguration()
            {
                UseDefaultCommandErrorHandler = false,
            });
            #endregion

            await builder.ConnectAsync();
            Client = builder.Build();
        }

        #region Messages
        public static async ValueTask sendMessageAsnyc(DiscordChannel channel, string content)
        {
            await channel.SendMessageAsync(content);
        }
        public static async ValueTask sendMessageAsnyc(DiscordChannel channel, DiscordMessageBuilder builder)
        {
            await channel.SendMessageAsync(builder);
        } 
        #endregion
    }
}