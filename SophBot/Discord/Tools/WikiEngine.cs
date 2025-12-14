using System.Data;
using DSharpPlus.Entities;
using SophBot.Discord.Tools.OptionProviders;
using SophBot.Universal;

namespace SophBot.Discord.Tools
{
    public static class WikiEngine
    {
        public static WikiArticle? getArticle(string article, int site)
        {
            DataTable data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Wiki, ["article", "site", "content"], [new ("article", "=", article)]);

            if (data.Rows.Count == 0)
                return null;

            DataRow? target = data.AsEnumerable().FirstOrDefault(row => (int)row["site"] == site);

            if (target == null)
                return null;

            return new()
            {
                Title = (string)target["article"],
                Content = (string)target["content"],
                CurrentSite = site,
                SiteCount = data.Rows.Count
            };
        }
        public static void setArticle(string article, int site, string content)
        {
            if (getArticle(article, site) == null)
            {
                DatabaseEngine.InsertData(
                    DatabaseEngine.DBTable.Wiki, 
                    new ()
                    {
                        {"article", article},
                        {"site", site},
                        {"content", content}
                    });
            }
            else
            {
                DatabaseEngine.ModifyData(
                    DatabaseEngine.DBTable.Wiki,
                    new () { {"content", content} },
                    [new ("article", "=", article), new ("site", "=", site)]
                    );
            }
        }
    }

    public class WikiArticle
    {
        public required string Title;
        public required string Content;
        public required int CurrentSite;
        public required int SiteCount;

        public DiscordContainerComponent AsContainerComponent()
        {
            DiscordComponent[] components =
            {
                new DiscordActionRowComponent([
                    new DiscordSelectComponent("wikiArtSelect", Title, WikiOptionProviders.ArticleProvider.GetSelectOptions()),
                ]),
                new DiscordTextDisplayComponent(Content),
                new DiscordSeparatorComponent(true),
                new DiscordActionRowComponent([
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wikiSitePrev", "Vorherige Seite", CurrentSite == 1 ? true : false),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wikiSiteCount", $"Seite {CurrentSite}/{SiteCount}", true),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wikiSiteNext", "Nächste Seite", CurrentSite == SiteCount ? true : false)
                ])
            };
            return new (components);
        }
    }
}