using DSharpPlus;
using SophBotv2.bot.conf;
using SophBotv2.bot.discord.events;
using SophBotv2.bot.logs;

namespace SophBotv2.bot.discord
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
                    events.AddEventHandlers<ErrorEvents>();
                    events.AddEventHandlers<ClientEvents>();
                });

                builder.ConfigureExtraFeatures(features =>
                {
                    features.LogUnknownAuditlogs = false;
                    features.LogUnknownEvents = false;
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