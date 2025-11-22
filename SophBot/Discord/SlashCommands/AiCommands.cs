using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using SophBot.Universal;

namespace SophBot.Discord.SlashCommands
{
    public class AiCommands
    {
        [Command("ai")]
        public async Task AiCmd(CommandContext ctx, [Description("Der Promt für die AI")] string promt)
        {
            await ctx.DeferResponseAsync();
            string response = await GeminiEngine.GenerateResponseAsync(new DiscordAiRequest(ctx.Channel, ctx.User, promt));
            List<DiscordComponent> components =
            [
                new DiscordTextDisplayComponent("### " + promt),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent(response),
            ];

            await ctx.RespondAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, false, DiscordColor.White)));
        }
    }
}