using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.Twitch;
using SophBot.Universal;
using TwitchSharp;

namespace SophBot.Discord.DebugCommands
{
    [Command("restart"), RequireApplicationOwner, RequirePermissions(DiscordPermission.Administrator), AllowedProcessors<TextCommandProcessor>]
    public class DebugRestartCommands
    {
        [Command("ai")]
        public async Task RestartAi(CommandContext ctx)
        {
            GeminiEngine.Initialize(Config.Ai.Token);
            GeminiEngine.StartNewMainChat();
            GeminiEngine.StartNewGoogleChat();
            Logs.AddLog("Restarted GeminiEngine", LogLevel.Information, "SophBot.Discord.DebugCommands");

            await ctx.RespondAsync("SophBot AI wurde neu gestartet!");
        }
        [Command("twitch")]
        public async Task RestartTwitch(CommandContext ctx, string authUri)
        {
            string refreshToken = await TwitchSharpEngine.GenerateRefreshTokenAsync(Config.Twitch.ClientId, Config.Twitch.ClientSecret, "https://localhost:3000", authUri);
            await TwitchEngine.Initialize(refreshToken);
            Logs.AddLog("Restarted TwitchEngine", LogLevel.Information, "SophBot.Discord.DebugCommands");

            await ctx.RespondAsync("Die TwitchEngine wurde neu gestartet!");
        }
    }
}