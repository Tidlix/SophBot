using SophBot.bot.conf;
using SophBot.bot.logs;
using SophBotv2.bot.discord;

namespace SophBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                SLogger.Log($"Unhandled Exception => {e.ExceptionObject.ToString()}", type: LogType.Error);
            };

            await SConfig.ReadConfigAsync();
            await SBotClient.CreateClientAsync();


            while (true);
        }   
    }
}