using System.Formats.Asn1;
using CommunityToolkit.HighPerformance.Helpers;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SophBot.Commands.ContextChecks;
using SophBot.Commands.ModCommands;
using SophBot.Commands.UsercCommands;
using SophBot.Configuration;
using SophBot.Database;
using SophBot.EventHandlers;
using SophBot.Messages;
using TwitchLib.Api;
using TwitchLib.Api.Services;

namespace SophBot.Configuration {
    public static class Clients {
        #pragma warning disable CA2211, CS8618
        public static DiscordClient DiscordClient;
        public static LiveStreamMonitorService TwitchMonitoring;

        public static async ValueTask CreateDiscordClient() {
            #region Create ClientBuilder
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(Config.Discord.Token, DiscordIntents.All);
            builder.ConfigureExtraFeatures(conf => {
                conf.LogUnknownAuditlogs = false;
                conf.LogUnknownEvents = false;
            }); 
            builder.ConfigureLogging(conf => {
                conf.SetMinimumLevel(LogLevel.Error);
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
                extension.AddCheck<RequireBotOwnerCheck>();

                extension.AddCommands([
                    typeof(DebugCommands),
                    typeof(ModerationCommands),
                    typeof(ModerationCommands.warnCommands),
                    typeof(RuleCommands),
                    typeof(ServerCommands),
                    typeof(ServerCommands.customCommands),
                    typeof(CustomCommands),
                    typeof(RRCommands),
                    typeof(TwitchCommands)
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
            });
            #endregion

            await builder.ConnectAsync();
            DiscordClient = builder.Build();
        }

        public static async ValueTask CreateTwitchMonitoring() {
            #pragma warning disable CS8629
            TwitchAPI api = new TwitchAPI();
            api.Settings.ClientId = Config.Twitch.ClientId;
            api.Settings.Secret = Config.Twitch.ClientSecret;
            api.Settings.AccessToken = Config.Twitch.AccessToken;

            TwitchMonitoring = new LiveStreamMonitorService(api, 10);
            TwitchMonitoring.OnStreamOnline += async (s, e) => {
                string twitchChannel = e.Channel;
                var time = e.Stream.StartedAt;

                List<ulong> discordChannels = await TidlixDB.getMonitoringChannels(twitchChannel.ToLower());

                foreach (ulong discordChannelId in discordChannels) {
                    DiscordChannel discordChannel = await DiscordClient.GetChannelAsync(discordChannelId);
                    DiscordGuild guild = await DiscordClient.GetGuildAsync((ulong)discordChannel.GuildId);
                    
                    ulong mentionRoleId = await TidlixDB.readServerconfig("mentionrole", guild.Id);
                    DiscordRole mentionRole = await guild.GetRoleAsync(mentionRoleId);

                    string url = e.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080");
                    DiscordComponent[] components = [
                        new DiscordTextDisplayComponent($"# {e.Stream.UserName} ist nun Live!"),
                        new DiscordSeparatorComponent(true),
                        new DiscordTextDisplayComponent($"## {e.Stream.Title}"),
                        new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(url, "test", false)),
                        new DiscordSeparatorComponent(true),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"**{mentionRole.Mention}** \n*{time.Date.Day}.{time.Date.Month}.{time.Date.Year} - {time.Hour}:{time.Minute}*"), new DiscordLinkButtonComponent($"https://twitch.tv/{twitchChannel}", label: "Jetzt auf Twitch.tv ansehen!"))
                    ];

                    var msg = new DiscordMessageBuilder()
                    .EnableV2Components()
                    .AddRawComponents(new DiscordContainerComponent(components, color: DiscordColor.Purple))
                    .WithAllowedMention(new RoleMention(mentionRole));

                    await discordChannel.SendMessageAsync(msg);
                }
            };
            
            var list = new List<string>();
            var channelList = await TidlixDB.getAllMonitorings();
            foreach(var channel in channelList) {
                list.Add(channel);
            }
            
            Clients.TwitchMonitoring.SetChannelsByName(list);
            TwitchMonitoring.Start();
        }
    }
}