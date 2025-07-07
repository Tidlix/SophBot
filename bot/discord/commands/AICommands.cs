using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.ai;
using SophBot.bot.logs;

namespace SophBot.bot.discord.commands
{
    [Command("AI")]
    public class AICommands
    {
        [Command("gemini"), Description("Leistungsstärker Modell, aber noch im Testmodus (Bevorzugt)")]
        public async ValueTask gemini(CommandContext ctx, string promt)
        {
            await ctx.DeferResponseAsync();
            SLogger.Log(LogLevel.Debug, $"Got Gemini command", "AICommands.cs");
            var components = new List<DiscordComponent>();
            components.Add(new DiscordTextDisplayComponent($"### {promt}"));
            components.Add(new DiscordSeparatorComponent(true));

            SLogger.Log(LogLevel.Debug, $"Try to get response for {promt}", "AICommands.cs");
            string response = await SGeminiEngine.GenerateResponseAsync($"{ctx.User.Username} sagt: {promt}");
            components.Add(new DiscordTextDisplayComponent(response));
            SLogger.Log(LogLevel.Debug, $"Got response - sending message", "AICommands.cs");

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components)));
        }
        [Command("groq"), Description("Schwächeres Modell, dafür schneller und zuverlässiger (Backup)")]
        public async ValueTask groq(CommandContext ctx, string promt)
        {
            await ctx.DeferResponseAsync();
            SLogger.Log(LogLevel.Debug, $"Got Groq command", "AICommands.cs");
            var components = new List<DiscordComponent>();
            components.Add(new DiscordTextDisplayComponent($"### {promt}"));
            components.Add(new DiscordSeparatorComponent(true));

            SLogger.Log(LogLevel.Debug, $"Try to get response for {promt}", "AICommands.cs");
            string response = await SGroqEngine.GenerateResponseAsync($"{ctx.User.Username} sagt: {promt}");
            components.Add(new DiscordTextDisplayComponent(response));
            SLogger.Log(LogLevel.Debug, $"Got response - sending message", "AICommands.cs");

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components)));
        }
    }
}