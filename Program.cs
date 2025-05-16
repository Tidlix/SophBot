using SophBot.Configuration;
using SophBot.Database;
using SophBot.Messages;
namespace SophBot;

public class Program
{
    static async Task Main(string[] args)
    {
        await Config.ReadAsnyc();

        try
        {
            await TidlixDB.createDB();
        }
        catch (Exception e)
        {
            await Log.sendMessage($"Couldn't connect to Database. Database will not be reachable! - {e.Message}", MessageType.Error());
        }
        await Services.Discord.CreateDiscordClient();
        await Services.Twitch.CreateTwitchMonitoring();

        while (true) ;
    }
}
