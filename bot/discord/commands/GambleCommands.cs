using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.commands
{
    public class GambleCommands
    {
        public enum Coin
        {
            Kopf,
            Zahl
        }
        [Command("Coinflip"), Description("Wirf eine Münze. Kopf -> Gewinnen, Zahl -> Verloren")]
        public async ValueTask CoinFlip(CommandContext ctx, Coin coin, ulong points)
        {
            await ctx.DeferResponseAsync();
            List<DiscordComponent> components = new();
            SDiscordUser user = new(ctx.Member!);

            ulong playerPoints = await user.GetPointsAsync();
            if (points > playerPoints)
            {
                await ctx.EditResponseAsync("Du hast nicht genug Punkte für diese Aktion!");
                return;
            }

            components.Add(new DiscordTextDisplayComponent("## Coinflip"));
            components.Add(new DiscordSeparatorComponent(true));

            if (new Random().Next(2) == 1)
            {
                components.Add(new DiscordTextDisplayComponent("**Die Münze zeigt: __Kopf__**"));
                if (coin == Coin.Kopf)
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast auch Kopf gewählt und somit gewonnen!"));
                    await user.AddPointsAsync(points);
                }
                else
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast Zahl gewählt und somit verloren!"));
                    await user.RemovePointsAsync(points);
                }
            }
            else
            {
                components.Add(new DiscordTextDisplayComponent("**Die Münze zeigt: __Zahl__**"));
                if (coin == Coin.Zahl)
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast auch Zahl gewählt und somit gewonnen!"));
                    await user.AddPointsAsync(points);
                }
                else
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast Kopf gewählt und somit verloren!"));
                    await user.RemovePointsAsync(points);
                }
            }
            playerPoints = await user.GetPointsAsync();
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"-# Du hast nun {playerPoints} Punkte!"));

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components, color: DiscordColor.Gold)));
        }
    }
}