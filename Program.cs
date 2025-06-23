using SophBot.bot.conf;
using SophBot.bot.logs;
using SophBot.bot.discord;
using SophBot.bot.database;
using Microsoft.Extensions.Logging;

namespace SophBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SConfig.LogLevel = LogLevel.Debug;

            await SConfig.ReadConfigAsync();
            await SDBEngine.Initialize();
            await SBotClient.CreateClientAsync();


            while (true) ;
        }   
    }
}