using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Configuration;
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

            var response = await Services.AI.Generate(promt);

            DiscordComponent[] components =
            {
                new DiscordTextDisplayComponent($"**\"{promt}\"**"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent(response)
            };
            DiscordContainerComponent container = new DiscordContainerComponent(components);
            
            var message = new DiscordMessageBuilder();
            message.EnableV2Components();
            message.AddRawComponents(container);


            await ctx.EditResponseAsync(message);
        }
    }
}
