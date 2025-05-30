using SophBot.Configuration;
using SophBot.Objects;


public class Program
{
    static async Task Main(string[] args)
    {
        await Config.ReadAsnyc();

        try
        {
            await TDatabase.createDB();
        }
        catch (Exception e)
        {
            TLog.sendLog($"Couldn't create Database. Database will not be reachable! > {e.Message} < ", TLog.MessageType.Warning);
        }

        await TBotClient.CreateDiscordClient(Microsoft.Extensions.Logging.LogLevel.Debug);
        await TTwitchClient.CreateTwitchMonitoring();
        

        while (true) ;
    }
}