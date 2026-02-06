using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SophBot.Discord.EventHandlers;
using SophBot.Universal;

namespace SophBot.Discord
{
    public static class DiscordEngine
    {
#pragma warning disable CS8618 
        public static DiscordClient Client { get; private set; }
        public static InteractivityExtension Interactivity { get; private set; }
#pragma warning restore CS8618 

        public static async Task Initialize(string token)
        {
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.All);
            builder.DisableDefaultLogging();
            builder.ConfigureLogging(l =>
            {
                l.ClearProviders();
                l.AddProvider(new Logs.LogProvider());
                l.SetMinimumLevel(LogLevel.Debug);
            });
            builder.ConfigureEventHandlers(e =>
            {
                e.AddEventHandlers<BasicMessageEventHandler>();
                e.AddEventHandlers<SystemMessageEventHandler>();
                e.AddEventHandlers<WikiEventHandler>();
            });
            builder.ConfigureExtraFeatures(f =>
            {
                f.LogUnknownAuditlogs = false;
                f.LogUnknownEvents = false;
            });

            builder.UseInteractivity();
            

            builder.UseCommands((IServiceProvider sp, CommandsExtension ce) =>
            {
                ce.AddCommands(Assembly.GetExecutingAssembly());
                ce.CommandErrored += async (s, e) =>
                {
                    try { await e.Context.RespondAsync("Ein Fehler ist aufgetreten!"); }
                    catch { await e.Context.FollowupAsync("Ein Fehler ist aufgetreten!"); }
                    throw new Exception($"Command ({e.Context.Command}) failed! {e.Exception.Message}");
                };

                TextCommandProcessor tcp = new(new()
                {
                    PrefixResolver = new DefaultPrefixResolver(false, "?").ResolvePrefixAsync
                });
                SlashCommandProcessor scp = new(new()
                {
                    NamingPolicy = new SnakeCaseNamingPolicy()
                });

                ce.AddProcessor(tcp);
                ce.AddProcessor(scp);
            },
            new CommandsConfiguration()
            {
                UseDefaultCommandErrorHandler = false,
            });

            Client = builder.Build();
            Interactivity =  (Client.ServiceProvider.GetService(typeof(InteractivityExtension)) as InteractivityExtension)!;
            await Client.ConnectAsync();
        }
    }
}