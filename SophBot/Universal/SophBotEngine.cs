using System.Diagnostics;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using SophBot.Twitch;
using TwitchSharp;

namespace SophBot.Universal
{
    public static class SophBotEngine
    {
        public static class Clients
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public static DiscordClient Discord;
            public static TwitchClient Twitch;
            public static LogLevel MinimumLogLevel = LogLevel.Information;
#pragma warning restore CS8618
        }


        public static void SendConsole(string content, LogLevel logLevel = LogLevel.Debug)
        {
            throw new NotImplementedException();
        }        
    }
}