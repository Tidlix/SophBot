using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SophBot.bot.discord.features;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class WikiEvents :
        IEventHandler<ComponentInteractionCreatedEventArgs>,
        IEventHandler<ModalSubmittedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            try
            {
                string customId = e.Interaction.Data.CustomId;
                if (!customId.Contains("wiki")) return;

                SLogger.Log(LogLevel.Debug, "Found Wiki-Interaction", "WikiEvents.cs");

                if (!customId.Contains("modify"))
                {
                    await e.Interaction.DeferAsync(true);
                    await e.Interaction.DeleteOriginalResponseAsync();
                }
                else
                {
                    if (!s.CurrentApplication.Owners!.Contains(e.Interaction.User))
                    {
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("`Du bist nicht berechtigt, diese Aktion auszufühen!`"));
                        return;
                    }
                }
                DiscordMessage message = e.Message;
                string article = "";
                int site = 0;

                if (e.Interaction.Data.ComponentType == DiscordComponentType.StringSelect)
                {
                    article = e.Interaction.Data.Values.First();
                    site = 0;
                    SLogger.Log(LogLevel.Debug, $"Got Article change {article}.{site}", "WikiEngine.cs");
                }

                if (e.Interaction.Data.ComponentType == DiscordComponentType.Button)
                {
                    string pattern = @"article=([^}]+);site=([^}]+);";

                    Match match = Regex.Match(customId, pattern);
                    if (match.Success)
                    {
                        article = match.Groups[1].Value;
                        int.TryParse(match.Groups[2].Value, out site);
                        SLogger.Log(LogLevel.Debug, $"Got Site change {article}.{site}", "WikiEngine.cs");
                    }
                    else
                    {
                        throw new Exception("Coudln't find match in customID!");
                    }
                }

            if (customId.Contains("modify"))
            {
                string? value = await WikiEngine.getSite(article, site);
                value ??= "";

                var modal = new DiscordInteractionResponseBuilder()
                    .WithTitle($"{article}.{site}")
                    .WithCustomId($"modify-wiki_article={article};site={site};")
                    .AddTextInputComponent(new DiscordTextInputComponent(
                        label: "Input",
                        customId: "articleInput",
                        value: value,
                        style: DiscordTextInputStyle.Paragraph));
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
                await e.Message.DeleteAsync();
                return;
                }

                var responseMsg = await WikiEngine.getWikiMessage(article, site);

                await message.ModifyAsync(responseMsg);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Warning, "Something went wrong at wiki article/site switch/modification", "WikiEvents.cs", ex);
                await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Ein Fehler ist aufgetreten! Bitte kontaktiere den Entwickler!"));
            }
        }

        public async Task HandleEventAsync(DiscordClient s, ModalSubmittedEventArgs e)
        {
            try
            {
                string customId = e.Interaction.Data.CustomId;
                if (!customId.Contains("wiki")) return;

                await e.Interaction.DeferAsync(true);

                SLogger.Log(LogLevel.Debug, "Found Wiki-Modification", "WikiEvents.cs");

                string article;
                int site;
                string input;

                string pattern = @"article=([^}]+);site=([^}]+);";

                Match match = Regex.Match(customId, pattern);
                if (match.Success)
                {
                    article = match.Groups[1].Value;
                    int.TryParse(match.Groups[2].Value, out site);
                    SLogger.Log(LogLevel.Debug, $"Got Site modification for {article}.{site}", "WikiEngine.cs");
                }
                else
                {
                    throw new Exception("Coudln't find match in customID!");
                }

                input = e.Values.Values.First();

                await WikiEngine.setSite(article, site, input);

                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Der Wiki-Eintrag wurde eingefügt!"));
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Warning, "Something went wrong at wiki article/site switch/modification", "WikiEvents.cs -> HandleEventAsync()", ex);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Ein Fehler ist aufgetreten! Bitte kontaktiere den Entwickler!"));
            }
        }
    }
}