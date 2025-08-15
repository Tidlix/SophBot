using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Converters;
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

            SDiscordUser user = new(ctx.Member!);
            ulong playerPoints = await user.GetPointsAsync();
            if (bet > playerPoints)
            {
                await ctx.EditResponseAsync("Du hast nicht genug Channelpoints für diese Aktion!");
                return;
            }

            var msg = await GambleEngine.playBlackJack(ctx.Member!, 0, 0, true, bet);
            await user.RemovePointsAsync(bet);

            await ctx.EditResponseAsync(msg);
        }
        

        [Command("Slots"), Description("Dreh an den Slots")]
        public async ValueTask Slots(CommandContext ctx, ulong bet)
        {
            await ctx.DeferResponseAsync();

            SDiscordUser user = new(ctx.Member!);
            ulong playerPoints = await user.GetPointsAsync();
            if (bet > playerPoints)
            {
                await ctx.EditResponseAsync("Du hast nicht genug Channelpoints für diese Aktion!");
                return;
            }
            
            await user.RemovePointsAsync(bet);

            var msg = new DiscordMessageBuilder().EnableV2Components();
            List<DiscordComponent> components = new();

            DiscordEmoji[] items = {
                DiscordEmoji.FromName(ctx.Client, ":gem:"),
                DiscordEmoji.FromName(ctx.Client, ":coin:"),
                DiscordEmoji.FromName(ctx.Client, ":four_leaf_clover:"),
                DiscordEmoji.FromName(ctx.Client, ":shamrock:"),
                DiscordEmoji.FromName(ctx.Client, ":blossom:"),
            };
            DiscordEmoji[] item = new DiscordEmoji[3];

            Random rand = new();


            for (int i = 0; i < item.Length; i++)
                item[i] = items[rand.Next(0, items.Length)];

            components.Add(new DiscordTextDisplayComponent("## Slots"));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent(@$"
3x {items[0]} - x20   |   3x {items[1]} - x12
3x {items[2]} - x08   |   3x {items[3]} - x07
2x {items[0]} - x04   |   2x {items[1]} - x02"
            ));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"° ° ° ° ° ° ° ° ° \n {item[0]} | {item[1]} | {item[2]}\n° ° ° ° ° ° ° ° ° "));

            if (item[0] == item[1] && item[0] == item[2])
            {
                if (item[0] == items[0])
                    await user.AddPointsAsync(bet * 20);
                if (item[0] == items[1])
                    await user.AddPointsAsync(bet * 15);
                if (item[0] == items[2])
                    await user.AddPointsAsync(bet * 09);
                if (item[0] == items[3])
                    await user.AddPointsAsync(bet * 07);
            }
            else if (item.Count(x => x == items[0]) == 2)
                await user.AddPointsAsync(bet * 4);
            else if (item.Count(x => x == items[1]) == 2)
                await user.AddPointsAsync(bet * 2);


            playerPoints = await user.GetPointsAsync();
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"-# Du hast nun {playerPoints} Channelpoints!"));

            msg.AddContainerComponent(new (components, false, DiscordColor.Gold));
            await ctx.EditResponseAsync(msg);
        }
    }
}