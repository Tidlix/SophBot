using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Discord.Tools;
using SophBot.Discord.Tools.OptionProviders;

namespace SophBot.Discord.SlashCommands
{
    public class WikiCommands
    {
        [Command("Wiki")]
        public async Task Wiki(SlashCommandContext ctx, [SlashAutoCompleteProvider<WikiOptionProviders.ArticleProvider>] string article = "Startseite")
        {
            await ctx.DeferResponseAsync();
            var wiki = WikiEngine.getArticle(article, 1);

            if (wiki == null)
            {
                await ctx.EditResponseAsync($"Ungültiger Wiki-Artikel `{article}`!");
                return;
            }

            await ctx.EditResponseAsync(new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(wiki.AsContainerComponent()));
        }


        [Command("ModifyWiki"), RequirePermissions(DiscordPermission.SendTtsMessages)]
        public class ModifyWiki
        {
            [Command("CreateArticle")]
            public async Task CreateArticle(SlashCommandContext ctx, string title)
            {
                var wiki = WikiEngine.getArticle(title, 1);
                if (wiki != null)
                {
                    await ctx.RespondAsync("Ein Artikel mit diesem Titel exestiert bereits!");
                    return;
                }

                var modal = new DiscordModalBuilder();
                modal.WithTitle($"## Wiki- {title}:1");
                modal.WithCustomId("wikiArtCreate");
                modal.AddTextInput(new DiscordTextInputComponent(
                    customId: "wikiArtCreateInput",
                    placeholder: "Hier könnte ihre Werbung stehen!",
                    style: DiscordTextInputStyle.Paragraph,
                    min_length: 1,
                    max_length: 3800),
                    "Content");
                modal.AddTextDisplay("-# Im Wiki kann jegliche Formatierung von Discord verwendet werden!\n-# https://support.discord.com/hc/en-us/articles/210298617-Markdown-Text-101-Chat-Formatting-Bold-Italic-Underline");
                await ctx.RespondWithModalAsync(modal);

                var response = (await DiscordEngine.Interactivity.WaitForModalAsync("wikiArtCreate", TimeSpan.FromDays(1))).Result;
                await response.Interaction.DeferAsync();

                WikiEngine.setArticle(title, 1, (response.Values["wikiArtCreateInput"] as TextInputModalSubmission)!.Value);
                await response.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Wiki Artikel `{title}` wurde erstellt!"));
            }
            [Command("AddPage")]
            public async Task AddPage(SlashCommandContext ctx, [SlashAutoCompleteProvider<WikiOptionProviders.ArticleProvider>] string article)
            {
                var current = WikiEngine.getArticle(article, 1);
                if (current == null)
                {
                    await ctx.RespondAsync($"Ungültiger Wiki-Artikel `{article}`!");
                    return;
                }

                var modal = new DiscordModalBuilder();
                modal.WithTitle($"## Wiki- {article}:{current.PageCount + 1}");
                modal.WithCustomId("wikiArtAdd");
                modal.AddTextInput(new DiscordTextInputComponent(
                    customId: "wikiArtAddInput",
                    placeholder: "Hier könnte ihre Werbung stehen!",
                    style: DiscordTextInputStyle.Paragraph,
                    min_length: 1,
                    max_length: 3800),
                    "Content");
                modal.AddTextDisplay("-# Im Wiki kann jegliche Formatierung von Discord verwendet werden!\n-# https://support.discord.com/hc/en-us/articles/210298617-Markdown-Text-101-Chat-Formatting-Bold-Italic-Underline");
                await ctx.RespondWithModalAsync(modal);

                var response = (await DiscordEngine.Interactivity.WaitForModalAsync("wikiArtAdd", TimeSpan.FromDays(1))).Result;
                await response.Interaction.DeferAsync();

                WikiEngine.setArticle(article, current.PageCount + 1, (response.Values["wikiArtAddInput"] as TextInputModalSubmission)!.Value);
                await response.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Die Seite {current.PageCount + 1} für den Artikel `{article}` wurde erstellt!"));
            }
            [Command("ModifyPage")]
            public async Task ModifyPage(SlashCommandContext ctx, [SlashAutoCompleteProvider<WikiOptionProviders.ArticleProvider>] string article, int page)
            {
                var current = WikiEngine.getArticle(article, page);
                if (current == null)
                {
                    current = WikiEngine.getArticle(article, 1);
                    if (current == null)
                        await ctx.RespondAsync($"Ungültiger Wiki-Artikel `{article}`!");
                    else
                        await ctx.RespondAsync($"Ungültige Seiten Nr. Bitte gib eine Zahl von 1-{current.PageCount} an!\n-# Wenn du eine neue Seite hinzufügen möchtest, nutze den Befehl `add_page`");

                    return;
                }

                var modal = new DiscordModalBuilder();
                modal.WithTitle($"## Wiki- {article}:{page}");
                modal.WithCustomId("wikiArtModify");
                modal.AddTextInput(new DiscordTextInputComponent(
                    customId: "wikiArtModifyInput",
                    placeholder: "Hier könnte ihre Werbung stehen!",
                    value: current.Content,
                    style: DiscordTextInputStyle.Paragraph,
                    min_length: 1,
                    max_length: 3800),
                    "Content");
                modal.AddTextDisplay("-# Im Wiki kann jegliche Formatierung von Discord verwendet werden!\n-# https://support.discord.com/hc/en-us/articles/210298617-Markdown-Text-101-Chat-Formatting-Bold-Italic-Underline");
                await ctx.RespondWithModalAsync(modal);

                var response = (await DiscordEngine.Interactivity.WaitForModalAsync("wikiArtModify", TimeSpan.FromDays(1))).Result;
                await response.Interaction.DeferAsync();

                WikiEngine.setArticle(article, page, (response.Values["wikiArtModifyInput"] as TextInputModalSubmission)!.Value);
                await response.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Die Seite {page} für den Artikel `{article}` wurde bearbeitet!"));
            }
        }
    }
}