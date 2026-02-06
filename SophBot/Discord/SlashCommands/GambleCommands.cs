using System;
using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Universal;

namespace SophBot.Discord.SlashCommands
{
    public class GambleCommands
    {
        public enum Coin
        {
            Kopf,
            Zahl
        }

        [Command("Coinflip"), Description("Wirf eine Münze. 50/50 Win-Chance")]
        public async Task Coinflip(CommandContext ctx, [Description("Kopf oder Zahl?")] Coin coin, [Description("Wie viel möchtest du Wetten?")] int bet)
        {
            await ctx.DeferResponseAsync();
            Profile player = new Profile(ctx.User.Id);

            if (!checkChannelPoints(player, bet))
            {
                if (bet <= 0)
                    await ctx.EditResponseAsync("Du musst mehr als 0 Wetten!");
                else
                    await ctx.EditResponseAsync("Du hast nicht genügen Channelpoints um so viel zu Wetten!");
                return;
            }

            Coin flip;
            bool playerWon;

            DiscordComponent[] components = new DiscordComponent[5];
            DiscordMessageBuilder message = new DiscordMessageBuilder()
                .EnableV2Components();

            components[0] = new DiscordTextDisplayComponent("## 🪙 Coinflip");
            components[1] = new DiscordTextDisplayComponent($"{ctx.Member!.Mention} setzt auf: {coin}");
            components[2] = new DiscordSeparatorComponent(true);
            components[3] = new DiscordTextDisplayComponent("Die Münze wird geworfen...");
            components[4] = new DiscordSeparatorComponent();

            message.AddContainerComponent(new (components, false, DiscordColor.Gray));
            await ctx.EditResponseAsync(message);

            await Task.Delay(3000);

            flip = (Coin)Random.Shared.Next(Enum.GetValues(typeof(Coin)).Length);
            components[3] = new DiscordTextDisplayComponent($"Die Münze zeigt: {flip}");
            message.ClearComponents();
            message.AddContainerComponent(new (components, false, DiscordColor.Gray));
            await ctx.EditResponseAsync(message);

            await Task.Delay(1000);

            playerWon = flip == coin;
            
            components[4] = new DiscordTextDisplayComponent($"**{ctx.Member.Mention} {(playerWon ? "gewinnt" : "verliert")}!**");
            

            message.ClearComponents();
            message.AddContainerComponent(new (components, false, playerWon ? DiscordColor.Green : DiscordColor.Red));
            await ctx.EditResponseAsync(message);

            if (playerWon) 
                player.AddChannelpoints(bet);
            else 
                player.RemoveChannelpoints(bet);
        }

        [Command("Slots"), Description("Dreh an den Slots")]
        public async Task Slots(CommandContext ctx, [Description("Wie viel möchtest du Wetten?")] int bet)
        {
            await ctx.DeferResponseAsync();
            Profile player = new Profile(ctx.User.Id);

            if (!checkChannelPoints(player, bet))
            {
                if (bet <= 0)
                    await ctx.EditResponseAsync("Du musst mehr als 0 Wetten!");
                else
                    await ctx.EditResponseAsync("Du hast nicht genügen Channelpoints um so viel zu Wetten!");
                return;
            }

            Profile profile = new Profile(ctx.User.Id);
            profile.RemoveChannelpoints(bet);
            DiscordComponent[] components = new DiscordComponent[5];
            DiscordMessageBuilder message = new DiscordMessageBuilder()
                .EnableV2Components();

            string[] emojis = new[] { "🍒", "🍋", "🔔", "⭐", "🍉" };
            string[] slots = new string[3] { "❔", "❔", "❔" };

            components[0] = new DiscordTextDisplayComponent("## 🎰 Slots");
            components[1] = new DiscordTextDisplayComponent($"{ctx.Member!.Mention} setzt {bet} Punkte an den Slots!");
            components[2] = new DiscordSeparatorComponent(true);
            components[3] = new DiscordTextDisplayComponent($"[{string.Join('|', slots)}]");
            components[4] = new DiscordSeparatorComponent();

            message.AddContainerComponent(new (components, false, DiscordColor.Gray));
            await ctx.EditResponseAsync(message);

            await Task.Delay(1000);
            slots[0] = emojis[Random.Shared.Next(emojis.Length)];
            components[3] = new DiscordTextDisplayComponent($"[{string.Join('|', slots)}]");
            message.ClearComponents();
            message.AddContainerComponent(new (components, false, DiscordColor.Gray));
            await ctx.EditResponseAsync(message);

            await Task.Delay(1000);
            slots[1] = emojis[Random.Shared.Next(emojis.Length)];
            components[3] = new DiscordTextDisplayComponent($"[{string.Join('|', slots)}]");
            message.ClearComponents();
            message.AddContainerComponent(new (components, false, DiscordColor.Gray));
            await ctx.EditResponseAsync(message);

            await Task.Delay(1000);
            slots[2] = emojis[Random.Shared.Next(emojis.Length)];
            components[3] = new DiscordTextDisplayComponent($"[{string.Join('|', slots)}]");
            message.ClearComponents();
            message.AddContainerComponent(new (components, false, DiscordColor.Gray));
            await ctx.EditResponseAsync(message);

            // compute multiplier: 3 of a kind = 5x (10x for ⭐), 2 of a kind = 2x, otherwise lose
            float multiplier;
            if (slots[0] == slots[1] && slots[1] == slots[2])
            {
                multiplier = slots[0] == "⭐" ? 10 : 5;
            }
            else if (slots[0] == slots[1] || slots[0] == slots[2] || slots[1] == slots[2])
            {
                multiplier = 1.5f;
            }
            else
            {
                multiplier = 0;
            }

            long points = (long)(bet * multiplier);
            if (points > 0)
                profile.AddChannelpoints(points);

            await Task.Delay(1500);
            bool playerWon = points > 0;
            components[4] = new DiscordTextDisplayComponent($"**{ctx.Member.Mention} erhält {(playerWon ? points.ToString() : "0")} Punkte!**");

            message.ClearComponents();
            message.AddContainerComponent(new (components, false, playerWon ? DiscordColor.Green : DiscordColor.Red));
            await ctx.EditResponseAsync(message);

        }
        private static bool checkChannelPoints(Profile profile, int points)
        {
            if (points <= 0) 
                return false;
            if (profile.Channelpoints < points)
                return false;
            if (profile.Channelpoints >= points)
                return true;
            
            throw new Exception($"Something went wrong when checking channelpoints from user (id:{profile.ID};channelpointsToBeChecked{points})");
        }
    }
}