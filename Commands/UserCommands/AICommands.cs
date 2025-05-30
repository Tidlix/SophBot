using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Configuration;
using SophBot.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophBot.Commands.UserCommands
{
    public class AICommands
    {
        [Command("AI"), Description("Frag die KI etwas")]
        public async ValueTask askAI (CommandContext ctx, string promt)
        {
            await ctx.DeferResponseAsync();

            var response = await TSophBotAI.generateResponse(promt);

            DiscordComponent[] components =
            {
                new DiscordTextDisplayComponent($"**\"{promt}\"**"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent(response)
            };
            DiscordContainerComponent container = new DiscordContainerComponent(components);
            
            var message = new DiscordMessageBuilder();
            message.EnableV2Components();
            message.AddContainerComponent(container);


            await ctx.EditResponseAsync(message);
        }
    }
}
