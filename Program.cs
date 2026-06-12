using SophBot.Discord;
using SophBot.Universal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DSharpPlus.Extensions;
using DSharpPlus;
using DSharpPlus.Commands;
using System.Reflection;
using SophBot.Discord.EventHandlers;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;

namespace SophBot
{
    public static class Program 
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static IHost _host;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static T GetService<T>()
        {
            return _host.Services.GetService<T>() ?? throw new Exception($"Requested service ({typeof(T)}) was not found or is currently not available!");
        }


        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(); 

            string[] sections = ["Discord", "Database", "Gemini", "Twitch"];
            foreach(string section in sections)
            {
                foreach(var kvp in builder.Configuration.GetSection(section).GetChildren())
                {
                    if (kvp.GetChildren().Count() >= 2) break;
                    Environment.SetEnvironmentVariable($"{section}.{kvp.Key}", kvp.Value);
                }
            }



            // DSharpPlus Services
            builder.Services
                .AddDiscordClient(Environment.GetEnvironmentVariable("Discord.Token") ?? throw new Exception("Discord Token not found!"), DSharpPlus.DiscordIntents.All)
                .Configure<DiscordConfiguration>(conf =>
                {
                    conf.LogUnknownAuditlogs = false;
                    conf.LogUnknownEvents = false;
                })
                .AddCommandsExtension((ctx, conf) =>
                {
                    conf.AddCommands(Assembly.GetExecutingAssembly());
                    conf.AddProcessor(new SlashCommandProcessor(new SlashCommandConfiguration()
                    {
                        NamingPolicy = new LowercaseNamingPolicy()
                    }));
                })
                .AddInteractivityExtension()
                .ConfigureEventHandlers(conf =>
                {
                    conf.AddEventHandlers<BasicMessageEventHandler>();
                    conf.AddEventHandlers<CustomCommandEventHandler>();
                    conf.AddEventHandlers<LogEventHandler>();
                    conf.AddEventHandlers<SystemMessageEventHandler>();
                    conf.AddEventHandlers<WikiEventHandler>();                    
                });

            // TwitchSharp Services
            //
            
            // SophBot Services
            builder.Services
                .AddSingleton<DiscordService>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<GeminiService>()
                .AddHostedService(s => s.GetRequiredService<DiscordService>())
                .AddHostedService(s => s.GetRequiredService<DatabaseService>())
                .AddHostedService(s => s.GetRequiredService<GeminiService>());

            builder.Logging
                .ClearProviders()
                .SetMinimumLevel(LogLevel.Debug)
                .AddProvider(new LoggingProvider(new DatabaseService()));
                
            _host = builder.Build();

            Console.WriteLine(@$"
# # # # # # # # 
 # # # # # # # 
  SOPHBOT V3
 # # # # # # # 
# # # # # # # #

Console currently disabled!");
            await _host.RunAsync();
        }
    }
}