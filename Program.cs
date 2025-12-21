using System.Data;
using SophBot.Discord;
using SophBot.Twitch;
using SophBot.Universal;

namespace SophBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Read Config
            Config.Read();

            // Initialize DB
            DatabaseEngine.Initialize();

            // Start AI
            GeminiEngine.Initialize(Config.Ai.Token);

            // Start Discord
            await DiscordEngine.Initialize(Config.Discord.Token);
            
            // Start Twitch
            await TwitchEngine.Initialize();


            while (true)
            {
                Console.Write("> ");
                string? command = Console.ReadLine();
                if (command is null or "") continue;

                if (command.StartsWith("STOP")) break;
                else
                {
                    string response = await GeminiEngine.GenerateResponseAsync(new ConsoleAiRequest(command));
                    Console.WriteLine("\n---------------------------------------------");
                    Console.WriteLine(response);
                    Console.WriteLine("---------------------------------------------\n");
                }
            }
        }   
    }
}