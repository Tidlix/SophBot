using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.Universal;

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
    }
}