using DSharpPlus.Entities;
using SophBot.bot.database;
using SophBot.bot.logs;

namespace SophBot.bot.discord.features.wiki
{
    public class WikiEngine
    {
        private static async ValueTask<List<DiscordSelectComponentOption>> getArticleList()
        {
            var articles = await SDBEngine.SelectAsync(SDBTable.Wiki, SDBColumn.Name);

            List<string> articleList = new();
            foreach (var article in articles) if (!articleList.Contains(article)) articleList.Add(article);

            List<DiscordSelectComponentOption> options = new();
            foreach (var article in articleList)
            {
                SLogger.Log("Got Wiki Article " + article, type: LogType.Debug);

                options.Add(new DiscordSelectComponentOption(article, article));
            }

            return options;
        }
        private static async ValueTask<string> getSite(string article, int site)
        {
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.Name, article));
            conditions.Add(new SDBValue(SDBColumn.Number, site.ToString()));

            var result = await SDBEngine.SelectAsync(SDBTable.Wiki, SDBColumn.Description, conditions, 1);

            SLogger.Log("Got Wiki Site " + result.First(), type: LogType.Debug);

            return result.First();
        }
        private static async ValueTask<int> countSites(string article)
        {
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.Name, article));

            var sites = await SDBEngine.SelectAsync(SDBTable.Wiki, SDBColumn.Number, conditions);
            SLogger.Log($"Found {sites.Count} sites for wiki article {article}", type: LogType.Debug);
            return sites.Count;
        }

        public static async ValueTask<DiscordMessageBuilder> getWikiMessage(string article, int site)
        {
            List<DiscordComponent> articleRow = new();
            articleRow.Add(new DiscordSelectComponent("wikiArticleSelect", article, await getArticleList()));

            List<DiscordComponent> siteRow = new();
            siteRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"wiki_article={article};site={site - 1};", "Vorherige Seite", (site <= 0) ? true : false));
            siteRow.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"wiki_article={article};site={site + 1};", "Nächste Seite", (site >= await countSites(article)-1) ? true : false));


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