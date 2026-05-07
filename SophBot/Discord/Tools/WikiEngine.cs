using System.Data;
using DSharpPlus.Entities;
using SophBot.Discord.Tools.OptionProviders;
using SophBot.Universal;

namespace SophBot.Discord.Tools
{
    public static class WikiEngine
    {
        public static WikiArticle? getArticle(string article, int page)
        {
            DataTable data = Program.GetService<DatabaseService>().SelectEntrys(DBTable.Wiki, ["article", "page", "content"], [new ("article", "=", article)]);

            if (data.Rows.Count == 0)
                return null;

            DataRow? target = data.AsEnumerable().FirstOrDefault(row => (int)row["page"] == page);

            if (target == null)
                return null;

            return new()
            {
                Title = (string)target["article"],
                Content = (string)target["content"],
                CurrentPage = page,
                PageCount = data.Rows.Count
            };
        }
        public static void setArticle(string article, int page, string content)
        {
            if (getArticle(article, page) == null)
            {
                Program.GetService<DatabaseService>().InsertData(
                    DBTable.Wiki, 
                    new ()
                    {
                        {"article", article},
                        {"page", page},
                        {"content", content}
                    });
            }
            else
            {
                Program.GetService<DatabaseService>().ModifyData(
                    DBTable.Wiki,
                    new () { {"content", content} },
                    [new ("article", "=", article), new ("page", "=", page)]
                    );
            }
        }
    }

    public class WikiArticle
    {
        public required string Title;
        public required string Content;
        public required int CurrentPage;
        public required int PageCount;

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
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wikiPagePrev", "Vorherige Seite", CurrentPage == 1 ? true : false),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wikiPageCount", $"Seite {CurrentPage}/{PageCount}", true),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "wikiPageNext", "Nächste Seite", CurrentPage == PageCount ? true : false)
                ])
            };
            return new (components);
        }
    }
}