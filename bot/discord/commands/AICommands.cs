using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.ai;
using SophBot.bot.logs;
using TwitchLib.Client.Extensions;

namespace SophBot.bot.discord.commands
{
    public class AICommands {
        [Command("AI"), Description("Rede mir der SophBot-AI!")]
        public async ValueTask AI(CommandContext ctx, string promt)
        {
            string user = "null";
            string response = "null";
            try
            {
                await ctx.DeferResponseAsync();

                user = ctx.User.Username;


                SLogger.Log(LogLevel.Debug, $"Got Gemini command", "AICommands.cs");
                var components = new List<DiscordComponent>();
                components.Add(new DiscordTextDisplayComponent($"### {promt}"));
                components.Add(new DiscordSeparatorComponent(true));

                SLogger.Log(LogLevel.Debug, $"Try to get response for {promt}", "AICommands.cs");
                response = await SGeminiEngine.GenerateResponseAsync(new AIRequest(ctx.User.Username, promt, AIResponseChannelType.Discord, 4000-promt.Length));
                components.Add(new DiscordTextDisplayComponent(response));
                SLogger.Log(LogLevel.Debug, $"Got response - sending message", "AICommands.cs");

                await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components)));
                SLogger.LogAi(user, promt, response, false);
            }
            catch (Exception ex)
            {
                SLogger.LogAi(user, promt, response, true, ex);
                throw;
            }

        }
    }
}