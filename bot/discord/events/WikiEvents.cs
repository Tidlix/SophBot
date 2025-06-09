using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.bot.discord.features.wiki;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class WikiEvents : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            try
            {
                string customId = e.Interaction.Data.CustomId;
                if (!customId.Contains("wiki")) return;

                SLogger.Log("Found Wiki-Interaction", type: LogType.Debug);

                await e.Interaction.DeferAsync(true);
                await e.Interaction.DeleteOriginalResponseAsync();

                DiscordMessage message = e.Message;
                string article = "";
                int site = 0;

                if (e.Interaction.Data.ComponentType == DiscordComponentType.StringSelect)
                {
                    article = e.Interaction.Data.Values.First();
                    site = 0;
                    SLogger.Log($"Got Article change {article}.{site}", type: LogType.Debug);
                }

                if (e.Interaction.Data.ComponentType == DiscordComponentType.Button)
                {
                    // wiki_article={article};site={site - 1};
                    string pattern = @"article=([^}]+);site=([^}]+);";

                    Match match = Regex.Match(customId, pattern);
                    if (match.Success)
                    {
                        article = match.Groups[1].Value;
                        int.TryParse(match.Groups[2].Value, out site);
                        SLogger.Log($"Got Site change {article}.{site}", type: LogType.Debug);
                    }
                    else
                    {
                        throw new Exception("Coudln't find match in customID!");
                    }
                }

                var responseMsg = await WikiEngine.getWikiMessage(article, site);

                await message.ModifyAsync(responseMsg);
            }
            catch (Exception ex)
            {
                await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Ein Fehler ist aufgetreten! Bitte kontaktiere den Entwickler!"));
                SLogger.Log("Something went wrong at wiki article/site switch", "WikiEvents.cs -> HandleEventAsync()", ex, LogType.Warning);
            }
        }
    }
}