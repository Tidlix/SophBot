using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace SophBot.bot.discord.features
{
    public class GambleEngine()
    {
        static Random random = new Random();

        public static async Task<DiscordMessageBuilder> playBlackJack(DiscordMember player, int pPoints, int bPoints, bool playersTurn, ulong bet)
        {
            List<DiscordComponent> components = new();
            List<DiscordComponent> actionRow = new();
            DiscordMessageBuilder result = new();
            bool gameFinished = false;
            bool playerWon = false;
            SDiscordUser user = new(player);

            components.Add(new DiscordTextDisplayComponent("## Black Jack"));
            components.Add(new DiscordSeparatorComponent(true));

            if (pPoints == 0 && bPoints == 0)
            {
                string draws = "";

                {
                    Card drawnCard = new();
                    int rank = (int)drawnCard.rank;
                    if (rank == 1 && pPoints + 11 <= 21) rank = 11;
                    pPoints += rank;
                    draws += $"**Du ziehst deine erste Karte:** {drawnCard.suit} {drawnCard.rank} (+{rank})\n";
                }
                {
                    Card drawnCard = new();
                    int rank = (int)drawnCard.rank;
                    if (rank == 1 && pPoints + 11 <= 21) rank = 11;
                    bPoints += rank;
                    draws += $"**Der Dealer zieht seine erste Karte:** {drawnCard.ToString()} (+{rank})\n";
                }
                {
                    Card drawnCard = new();
                    int rank = (int)drawnCard.rank;
                    if (rank == 1 && pPoints + 11 <= 21) rank = 11;
                    pPoints += rank;
                    draws += $"**Du ziehst deine zweite Karte:** {drawnCard.ToString()} (+{rank})\n";
                }
                draws += $"Die zweite Karte des Dealers bleibt vorerst im Stapel\n";
                components.Add(new DiscordTextDisplayComponent(draws));
            }
            else
            {
                if (playersTurn)
                {
                    Card drawnCard = new();
                    int rank = (int)drawnCard.rank;
                    if (rank == 1 && pPoints + 11 <= 21) rank = 11;
                    pPoints += rank;
                    components.Add(new DiscordTextDisplayComponent($"**Du ziehst eine Karte: {drawnCard.ToString()} (+{rank})**\n"));

                }
                else
                {
                    string draws = "";
                    while (bPoints < 21 && bPoints < pPoints)
                    {
                        Card drawnCard = new();
                        int rank = (int)drawnCard.rank;
                        if (rank == 1 && pPoints + 11 <= 21) rank = 11;
                        bPoints += rank;
                        draws += $"**Der Dealer zieht eine Karte: {drawnCard.ToString()} (+{rank})**\n";
                    }
                    components.Add(new DiscordTextDisplayComponent(draws));
                }
            }

            

            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"**{player.Mention}'s Punkte: ** {pPoints}\n**Dealer's Punkte: ** {bPoints}"));


            if (pPoints > 21)
            {
                components.Add(new DiscordTextDisplayComponent("**Du hast mehr als 21 Punkte und somit verloren!**"));
                playerWon = false;
                gameFinished = true;
            }
            else if (bPoints > 21)
            {
                components.Add(new DiscordTextDisplayComponent("**Der Dealer hat mehr als 21 Punkte. Somit hast du gewonnen!**"));
                gameFinished = true;
                playerWon = true;
                await user.AddPointsAsync(bet * 2);
            }
            else if (!playersTurn)
            {
                components.Add(new DiscordTextDisplayComponent("**Der Dealer hat mehr Punkte als du. Somit hast du verloren!**"));
                playerWon = false;
                gameFinished = true;
            }
            else
            {
                components.Add(new DiscordTextDisplayComponent("Möchtest du eine weitere Karte?"));
            }
            components.Add(new DiscordSeparatorComponent(true));

            if (!gameFinished)
            {
                actionRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Success, $"blackjackDraw_owner={player.Id};ppoints={pPoints};bpoints={bPoints};bet={bet};", "Karte Ziehen"));
                actionRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Success, $"blackjackHold_owner={player.Id};ppoints={pPoints};bpoints={bPoints};bet={bet};", "Karten Halten"));
                components.Add(new DiscordActionRowComponent(actionRow));
                components.Add(new DiscordSeparatorComponent(true));
            }
            

            result.EnableV2Components().AddContainerComponent(new(components, false, (gameFinished) ? ((playerWon) ? DiscordColor.Green : DiscordColor.Red) : DiscordColor.Goldenrod));
            return result;
        }


        internal class Card
        {
            public enum Rank
            {
                Ace = 1,
                Two = 2,
                Three = 3,
                Four = 4,
                Five = 5,
                Six = 6,
                Seven = 7,
                Eight = 8,
                Nine = 9,
                Ten = 10,
                Jack = 10,
                Queen = 10,
                King = 10
            }
            public enum Suit
            {
                Hearts,
                Diamonds,
                Clubs,
                Spades
            }

            public Rank rank;
            public Suit suit;
            public Card()
            {
                Array ranks = Enum.GetValues(typeof(Rank));
                Array suits = Enum.GetValues(typeof(Suit));

                this.rank = (Rank)ranks.GetValue(random.Next(ranks.Length))!;
                this.suit = (Suit)suits.GetValue(random.Next(suits.Length))!;
            }
            public Card(Rank Rank, Suit Suit)
            {
                this.rank = Rank;
                this.suit = Suit;
            }

            #pragma warning disable CS0114
            public string ToString()
            {
                return $"{suit} {rank}";
            }
        }
    }
}
