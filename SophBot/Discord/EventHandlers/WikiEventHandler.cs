using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Discord.Tools;
using System.Text.RegularExpressions;

namespace SophBot.Discord.EventHandlers
{
    public class WikiEventHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            if (!e.Id.StartsWith("wiki")) return;
            WikiArticle? wiki = null;

            if (e.Interaction.Data.ComponentType == DiscordComponentType.StringSelect)
            {
                string article = e.Interaction.Data.Values.First();
                wiki = WikiEngine.getArticle(article, 1);
            }
            if (e.Interaction.Data.ComponentType == DiscordComponentType.Button)
            {
                string article = "";
                int currentPage = 0;

                DiscordContainerComponent container = (e.Message.Components!.First() as DiscordContainerComponent)!;
                foreach (DiscordComponent current in container.Components)
                {
                    if (current is DiscordActionRowComponent action)
                    {
                        foreach (DiscordComponent current2 in action.Components)
                        {
                            if (current2 is DiscordSelectComponent select)
                            {
                                if (select.CustomId == "wikiArtSelect")
                                {
                                    article = select.Placeholder;
                                }
                            }

                            if (current2 is DiscordButtonComponent button)
                            {
                                if (button.CustomId == "wikiPageCount")
                                {
                                    string label = button.Label;
                                    Match match = Regex.Match(label, @"Seite (\d+)/");
                                    if (match.Success)
                                    {
                                        currentPage = int.Parse(match.Groups[1].Value);
                                    }
                                }
                            }
                        }
                    }
                }
                if (e.Id.Contains("Prev"))
                {
                    wiki = WikiEngine.getArticle(article, currentPage - 1);
                }
                else if (e.Id.Contains("Next"))
                {
                    wiki = WikiEngine.getArticle(article, currentPage + 1);
                }
            }
            if (wiki == null) return;

            await e.Interaction.CreateResponseAsync(
                    DiscordInteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .EnableV2Components()
                        .AddContainerComponent(wiki.AsContainerComponent()
                        ));
        }
    }
}