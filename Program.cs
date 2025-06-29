using SophBot.bot.conf;
using SophBot.bot.logs;
using SophBot.bot.discord;
using SophBot.bot.database;
using Microsoft.Extensions.Logging;
using SophBot.bot.ai;
using SophBot.bot.twitch;
using System.Diagnostics;

namespace SophBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SConfig.LogLevel = LogLevel.Information;
            await SConfig.ReadConfigAsync();
            await SDBEngine.Initialize();

            SGeminiEngine.StartSession();
            await SBotClient.CreateClientAsync();
            await STwitchClient.CreateTwitchMonitoringAsnyc();


            while (true) ;
        }   
    }
}