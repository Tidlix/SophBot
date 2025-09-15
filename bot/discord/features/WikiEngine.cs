using DSharpPlus.Entities;
using SophBot.bot.database;
using SophBot.bot.logs;
using Microsoft.Extensions.Logging;

namespace SophBot.bot.discord.features
{
    public class WikiEngine
    {
        private static async ValueTask<List<DiscordSelectComponentOption>> getArticleList()
        {
            var articlesRaw = await SDBEngine.SelectFromAsync("wiki", ["title"]);

            if (articlesRaw == null || articlesRaw.Length == 0)
                return new List<DiscordSelectComponentOption>() { new DiscordSelectComponentOption("404 Not Found", "") };

            var articleList = articlesRaw.Select(r => r[0]?.ToString() ?? string.Empty).Distinct().ToList();
            List<DiscordSelectComponentOption> options = new();

            foreach (var article in articleList)
            {
                SLogger.Log(LogLevel.Debug, $"Got Wiki Article {article}", "WikiEngine.cs");
                options.Add(new DiscordSelectComponentOption(article, article));
            }

            return options;
        }

        public static async ValueTask<string> getSite(string article, int site)
        {
            var conditions = new Dictionary<string, object>
            {
                { "title", article },
                { "site", site }
            };

            var result = await SDBEngine.SelectFromAsync(
                "wiki",
                new string[] { "value" },
                conditions: conditions,
                limit: 1);

            var description = result.FirstOrDefault()?[0]?.ToString() ?? string.Empty;
            SLogger.Log(LogLevel.Debug, $"Got Wiki Site {description}", "WikiEngine.cs");
            return description;
        }

        public static async ValueTask setSite(string article, int site, string input)
        {
            var conditions = new Dictionary<string, object>
            {
                { "title", article },
                { "site", site }
            };

            try
            {
                await SDBEngine.DeleteFromAsync("wiki", conditions);
            }
            catch {}

            var values = new Dictionary<string, object>
            {
                { "title", article },
                { "site", site },
                { "value", input }
            };

            await SDBEngine.InsertToAsync("wiki", [values]);
        }

        private static async ValueTask<int> countSites(string article)
        {
            var conditions = new Dictionary<string, object>
            {
                { "title", article }
            };

            var sites = await SDBEngine.SelectFromAsync("wiki", ["site"] , conditions);
            SLogger.Log(LogLevel.Debug, $"Found {sites.Length} sites for wiki article {article}", "WikiEngine.cs");
            return sites.Length;
        }

        public static async ValueTask<DiscordMessageBuilder> getWikiMessage(string article, int site)
        {
            List<DiscordComponent> articleRow = new();
            articleRow.Add(new DiscordSelectComponent("wikiArticleSelect", article, await getArticleList()));

            List<DiscordComponent> siteRow = new();
            siteRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Primary, $"wiki_article={article};site={site - 1};", "Vorherige Seite", site <= 0));
            siteRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wiki_article_site", $"Seite {site + 1}", true));
            siteRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Primary, $"wiki_article={article};site={site + 1};", "Nächste Seite", site >= await countSites(article) - 1));

            List<DiscordComponent> components = new();
            components.Add(new DiscordTextDisplayComponent("## Soph-Wiki"));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordActionRowComponent(articleRow));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent(await getSite(article, site)));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordActionRowComponent(siteRow));

            return new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new DiscordContainerComponent(components));
        }
    }
}