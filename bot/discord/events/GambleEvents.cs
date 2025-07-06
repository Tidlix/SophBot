using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.bot.discord.features;

namespace SophBot.bot.discord.events
{
    public class GambleEvents : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            string id = e.Id;
            if (!id.Contains("blackjack")) return;

            var ownerMatch = Regex.Match(id, @"owner=(\d+)");
            var pPointsMatch = Regex.Match(id, @"ppoints=(\d+)");
            var bPointsMatch = Regex.Match(id, @"bpoints=(\d+)");
            var betMatch = Regex.Match(id, @"bet=(\d+)");

            if (ownerMatch.Success && pPointsMatch.Success && bPointsMatch.Success && betMatch.Success)
            {
                ulong owner = ulong.Parse(ownerMatch.Groups[1].Value);
                int pPoints = int.Parse(pPointsMatch.Groups[1].Value);
                int bPoints = int.Parse(bPointsMatch.Groups[1].Value);
                ulong bet = ulong.Parse(betMatch.Groups[1].Value);

                DiscordMember player = await e.Guild!.GetMemberAsync(owner);

                var msg = await GambleEngine.playBlackJack(player, pPoints, bPoints, id.Contains("Draw") ? true : false, bet);

                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new (msg));
            }
        }
    }
}