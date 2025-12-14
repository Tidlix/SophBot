using System.Data;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using SophBot.Universal;

namespace SophBot.Discord.Tools.OptionProviders
{
    public class WikiOptionProviders
    {
        public class ArticleProvider : IAutoCompleteProvider
        {
            public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
            {
                var result = new List<DiscordAutoCompleteChoice>();
                var articles = new List<string>();
                var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Wiki, ["article"], [new("article", "LIKE", $"%{context.UserInput ?? ""}%")]);

                foreach(DataRow currentRow in data.Rows)
                {
                    string article = (string)currentRow["article"];
                    if (!articles.Contains(article))
                    {
                        articles.Add(article);
                        result.Add(new(article, article));
                    }
                }
                if (result.Count == 0) result.Add(new ("Invalid Wiki article!", null));
                return result; 
            }
            public static IEnumerable<DiscordSelectComponentOption> GetSelectOptions() 
            {
                var result = new List<DiscordSelectComponentOption>();
                var articles = new List<string>();
                var data = DatabaseEngine.SelectEntrys(DatabaseEngine.DBTable.Wiki, ["article"], [new("article", "LIKE", "%")]);

                foreach(DataRow currentRow in data.Rows)
                {
                    string article = (string)currentRow["article"];
                    if (!articles.Contains(article))
                    {
                        articles.Add(article);
                        result.Add(new(article, article));
                    }
                } 
                return result;
            }
        }
    }
}