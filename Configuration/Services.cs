using System.Formats.Asn1;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
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
using SophBot.Commands.UserCommands;
using SophBot.Configuration;
using SophBot.Database;
using SophBot.EventHandlers;
using SophBot.Messages;
using TwitchLib.Api;
using TwitchLib.Api.Services;

namespace SophBot.Configuration {
    public static class Services
    {
#pragma warning disable CA2211, CS8618
        
        

        #region Discord
        public static class Discord
        {
            public static DiscordClient Client;
            public static async ValueTask CreateDiscordClient()
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
                    //typeof(ServerCommands.customCommands),
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
                }, new CommandsConfiguration()
                {
                    UseDefaultCommandErrorHandler = false,
                });
                #endregion

                await builder.ConnectAsync();
                Client = builder.Build();
            }
        }
        #endregion


        #region Twitch
        public static class Twitch
        {
            public static LiveStreamMonitorService Monitoring;

            public static async ValueTask CreateTwitchMonitoring()
            {
#pragma warning disable CS8629
                TwitchAPI api = new TwitchAPI();
                api.Settings.ClientId = Config.Twitch.ClientId;
                api.Settings.Secret = Config.Twitch.ClientSecret;
                api.Settings.AccessToken = Config.Twitch.AccessToken;

                Monitoring = new LiveStreamMonitorService(api, 60);
                Monitoring.OnStreamOnline += async (s, e) =>
                {
                    string twitchChannel = e.Channel;
                    var time = e.Stream.StartedAt;

                    List<ulong> discordChannels = await TidlixDB.getMonitoringChannels(twitchChannel.ToLower());

                    foreach (ulong discordChannelId in discordChannels)
                    {
                        DiscordChannel discordChannel = await Discord.Client.GetChannelAsync(discordChannelId);
                        DiscordGuild guild = await Discord.Client.GetGuildAsync((ulong)discordChannel.GuildId);

                        ulong mentionRoleId = await TidlixDB.readServerconfig("mentionrole", guild.Id);
                        DiscordRole mentionRole = await guild.GetRoleAsync(mentionRoleId);

                        string url = e.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080");

                        
                        DiscordComponent[] components = [
                            new DiscordTextDisplayComponent($"# {e.Stream.UserName} ist nun Live!"),
                            new DiscordSeparatorComponent(true),
                            new DiscordTextDisplayComponent($"## {e.Stream.Title}"),
                            new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(url, "test", false)),
                            new DiscordSeparatorComponent(true),
                            new DiscordSectionComponent(new DiscordTextDisplayComponent($"**{mentionRole.Mention}** \n*{time.ToString("dd.MM.yyyy - HH:mm")}*"), new DiscordLinkButtonComponent($"https://twitch.tv/{twitchChannel}", label: "Jetzt auf Twitch.tv ansehen!"))
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
                foreach (var channel in channelList)
                {
                    list.Add(channel);
                }

                Monitoring.SetChannelsByName(list);
                Monitoring.Start();
            }
        }
        #endregion


        #region AI
        internal class AI
        {
            public static async ValueTask<string> Generate(string promt)
            {
                string url = "https://api.groq.com/openai/v1/chat/completions";
                var model = "compound-beta";

                promt = promt.Replace('"', ' ');

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.AI.Key);

                    // {{""role"": ""system"",""content"": ""Wenn dich jemand nach der gegebenen Frage befragt, gibst du nur das aus ""}},

                    var json = $@"{{
                ""messages"": [
                    {{""role"": ""system"",""content"": ""Das ist eine System Anweisung: Dein Name ist SophBot AI""}},
                    {{""role"": ""system"",""content"": ""Das ist eine System Anweisung: Tidlix ist dein Besitzer""}},
                    {{""role"": ""system"",""content"": ""Das ist eine System Anweisung: Nutze maximal 2000 Zeichen""}},
                    {{ ""role"": ""user"", ""content"": ""{promt}"" }}],
                ""model"": ""{model}""
                }}";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var result = await response.Content.ReadAsStringAsync();


                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(result))
                        {
                            var root = doc.RootElement;

                            var reply = root
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

                            if (reply == null) return "AI-Anfrage fehlgeschlagen - Bitte kontaktiere den Entwickler dieses Bots! (reply is null)";

                            return reply;
                        }
                    }
                    catch (Exception e)
                    {
                        await Log.sendMessage("AI Error: " + e.Message, MessageType.Error());
                        await Log.sendMessage("Promt: " + promt, MessageType.Error());
                        await Log.sendMessage("Response: " + response, MessageType.Error());

                        return "AI-Anfrage fehlgeschlagen - Bitte kontaktiere den Entwickler dieses Bots!";
                    }

                }
            }
        }
        #endregion
    }
}