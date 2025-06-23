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
            var articles = await SDBEngine.SelectAsync(SDBTable.Wiki, SDBColumn.Name);

            if (articles == null) return new List<DiscordSelectComponentOption>() { new DiscordSelectComponentOption("404 Not Found", "") };

            List<string> articleList = new();
            foreach (var article in articles) if (!articleList.Contains(article)) articleList.Add(article);

            List<DiscordSelectComponentOption> options = new();
            foreach (var article in articleList)
            {
                SLogger.Log(LogLevel.Debug, "Got Wiki Article " + article, "WikiEngine.cs");

                options.Add(new DiscordSelectComponentOption(article, article));
            }

            return options;
        }
        public static async ValueTask<string> getSite(string article, int site)
        {
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.Name, article));
            conditions.Add(new SDBValue(SDBColumn.Number, site.ToString()));

            var result = await SDBEngine.SelectAsync(SDBTable.Wiki, SDBColumn.Description, conditions, limit: 1);

            SLogger.Log(LogLevel.Debug, "Got Wiki Site " + result!.First(), "WikiEngine.cs");

            return result!.First();
        }
        public static async ValueTask setSite(string article, int site, string input)
        {
            List<SDBValue> values = new();
            values.Add(new SDBValue(SDBColumn.Name, article));
            values.Add(new SDBValue(SDBColumn.Number, site.ToString()));

            try
            {
                await SDBEngine.DeleteAsync(SDBTable.Wiki, values);    
            } catch {}
            

            values.Add(new SDBValue(SDBColumn.Description, input));

            await SDBEngine.InsertAsync(values, SDBTable.Wiki);
        }
        private static async ValueTask<int> countSites(string article)
        {
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.Name, article));

            var sites = await SDBEngine.SelectAsync(SDBTable.Wiki, SDBColumn.Number, conditions);
            SLogger.Log(LogLevel.Debug, $"Found {sites!.Count} sites for wiki article {article}", "WikiEngine.cs");
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