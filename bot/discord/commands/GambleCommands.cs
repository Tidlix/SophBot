using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.bot.discord.features;

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
        public async ValueTask CoinFlip(CommandContext ctx, Coin coin, ulong bet)
        {
            await ctx.DeferResponseAsync();
            List<DiscordComponent> components = new();
            SDiscordUser user = new(ctx.Member!);

            ulong playerPoints = await user.GetPointsAsync();
            if (bet > playerPoints)
            {
                await ctx.EditResponseAsync("Du hast nicht genug Channelpoints für diese Aktion!");
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
                    await user.AddPointsAsync(bet);
                }
                else
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast Zahl gewählt und somit verloren!"));
                    await user.RemovePointsAsync(bet);
                }
            }
            else
            {
                components.Add(new DiscordTextDisplayComponent("**Die Münze zeigt: __Zahl__**"));
                if (coin == Coin.Zahl)
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast auch Zahl gewählt und somit gewonnen!"));
                    await user.AddPointsAsync(bet);
                }
                else
                {
                    components.Add(new DiscordTextDisplayComponent("Du hast Kopf gewählt und somit verloren!"));
                    await user.RemovePointsAsync(bet);
                }
            }
            playerPoints = await user.GetPointsAsync();
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"-# Du hast nun {playerPoints} Channelpoints!"));

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new(components, color: DiscordColor.Gold)));
        }

        [Command("Blackjack"), Description("Spiele eine Runde Blackjack (leicht abgeänderte Version)")]
        public async ValueTask BlackJack(CommandContext ctx, ulong bet)
        {
            await ctx.DeferResponseAsync();
            var msg = await GambleEngine.playBlackJack(ctx.Member!, 0, 0, true, bet);
            SDiscordUser user = new(ctx.Member!);
            await user.RemovePointsAsync(bet);

            await ctx.EditResponseAsync(msg);
        }
    }
}