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
        public async Task AiCmd(CommandContext ctx, [Description("Der Prompt für die AI")] string prompt)
        {
            await ctx.DeferResponseAsync();
            string response = await Program.GetService<GeminiService>().GenerateResponseAsync(new DiscordAiRequest(ctx.Channel, ctx.User, prompt));
            List<DiscordComponent> components =
            [
                new DiscordTextDisplayComponent("### " + prompt),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent(response),
            ];

            await ctx.RespondAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components, false, DiscordColor.White)));
        }
    }
}