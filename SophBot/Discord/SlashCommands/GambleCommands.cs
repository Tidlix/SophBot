using System;
using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SophBot.Universal;

namespace SophBot.Discord.SlashCommands
{
    //[Command("gamble")] // Abstimmung im Gange -> https://discord.com/channels/1345374873186209814/1385635946443177994/1453864312711020544
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