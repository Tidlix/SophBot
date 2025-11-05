using DSharpPlus.Commands;
using DSharpPlus.Entities;
using GenerativeAI;
using GenerativeAI.Tools;
using System.Data;

namespace SophBot.Universal
{
    public class GeminiEngine
    {
#pragma warning disable CS8618
        public static GoogleAi GoogleAI { get; private set; }
        public static GenerativeModel MainModel { get; private set; }
        public static GenerativeModel GoogleModel { get; private set; }
        public static ChatSession MainChat { get; private set; }
        public static ChatSession GoogleChat { get; private set; }
        private static string MemoryFile = $"{AppDomain.CurrentDomain.BaseDirectory}/ai/memory.txt";
#pragma warning restore CS8618

        public static void Initialize(string token)
        {
            GoogleAI = new GoogleAi(token);
            MainModel = GoogleAI.CreateGenerativeModel("models/gemini-2.5-flash");
            GoogleModel = GoogleAI.CreateGenerativeModel("models/gemini-2.5-flash");

            MainModel.UseGoogleSearch = false;
            MainModel.SystemInstruction = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/ai/promt.txt");

            MainModel.EnableFunctions();
            MainModel.AddFunctionTool(new QuickTool(() => ReadMemory(), "ReadMemory", "Lies die gespeicherten Informationen"));
            MainModel.AddFunctionTool(new QuickTool((string content) => WriteMemory(content), "WriteMemory", "Speichere eine neue Information"));
            MainModel.AddFunctionTool(new QuickTool((long id, string newContent) => ModifyMemory(id, newContent), "ModifyMemory", "Bearbeite eine Information anhand der id (readMemory)"));
            MainModel.AddFunctionTool(new QuickTool((long id) => DeleteMemory(id), "DeleteMemory", "Lösche eine Information anhand der id (readMemory)"));
            MainModel.AddFunctionTool(new QuickTool(() => ReadWiki(), "ReadWiki", "Erhalte die Informationen des Internen Soph-Wikis"));
            MainModel.AddFunctionTool(new QuickTool((string request) => AskGoogleAi(request), "AskGoogleAi", "Frage ein KI-Modell, mit der möglichkeit google zu durchsuchen, nach Informationen"));


            GoogleModel.UseGoogleSearch = true;

            StartNewMainChat();
            StartNewGoogleChat();

            GoogleChat.DisableFunctions();

            GenerateResponseAsync(new AiRequest("liese deine gespeicherten Informationen - Gib diese nicht aus und warte auf weitere Anfragen")).Wait();
        }

        public static void StartNewMainChat()
        {
            MainChat = MainModel.StartChat();
        }
        public static void StartNewGoogleChat()
        {
            GoogleChat = GoogleModel.StartChat();
        }

        public static async Task<string> GenerateResponseAsync(AiRequest request)
        {
            return (await MainChat.GenerateContentAsync(request.ToString())).Text;
        }



        #region Function Tools
        private static string ReadMemory()
        {
            string result = string.Empty;
            DataTable data = DatabaseEngine.SelectTable(DatabaseEngine.DBTable.AI_Memory, "id");

            result += string.Join(" | ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            result += "\n";

            foreach (DataRow row in data.Rows)
            {
                result += string.Join(" | ", row.ItemArray);
                result += "\n";
            }
            return result;
        }
        private static string WriteMemory(string content)
        {
            DatabaseEngine.InsertData(DatabaseEngine.DBTable.AI_Memory, new Dictionary<string, object> { { "content", content } });
            return $"Erfolgreich! Neue Informations-Tabelle: \n" + ReadMemory();
        }
        private static string ModifyMemory(long id, string newContent)
        {
            DatabaseEngine.ModifyData(DatabaseEngine.DBTable.AI_Memory, new Dictionary<string, object> { { "content", newContent } }, [new("id", "=", id)]);
            return $"Erfolgreich! Neue Informations-Tabelle: \n" + ReadMemory();
        }
        private static string DeleteMemory(long id)
        {
            DatabaseEngine.DeleteData(DatabaseEngine.DBTable.AI_Memory, [new("id", "=", id)]);
            return $"Erfolgreich! Neue Informations-Tabelle: \n" + ReadMemory();
        }

        private static string ReadWiki()
        {
            string result = string.Empty;
            DataTable data = DatabaseEngine.SelectTable(DatabaseEngine.DBTable.Wiki, "article");

            result += string.Join(" | ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            result += "\n";

            foreach (DataRow row in data.Rows)
            {
                result += string.Join(" | ", row.ItemArray);
                result += "\n";
            }
            return result;
        }
        
        private static string AskGoogleAi(string request)
        {
            try
            {
                return GoogleChat.GenerateContentAsync(request).Result.Text;
            }
            catch
            {
                return "Google Request failed!";
            }
        }
        

        #endregion
    }
    #region Requests
    public class AiRequest
    {
        public SourceType Source;
        public ulong Channel;
        public bool IsPrivate { get; private set; }
        public string Name { get; private set; }
        public long Id { get; private set; }
        public string Promt { get; private set; }



        public AiRequest(string promt, bool isConsole = false)
        {
            Source = SourceType.Console;
            Id = isConsole ? 0 : -1;
            Name = isConsole ? "CONSOLE" : "SYSTEM";
            Promt = promt;
            IsPrivate = true;
        }
        public AiRequest(DiscordUser discordUser, DiscordChannel channel, string promt)
        {
            Source = SourceType.Discord;
            Channel = channel.Id;
            IsPrivate = channel.IsPrivate;
            Name = discordUser.GlobalName;
            Id = 404;
            Promt = promt;
        }

#pragma warning disable CS0114
        public string ToString()
        {
            int maxChars = 0;
            switch(Source)
            {
                case SourceType.Console:
                    maxChars = -1;
                    break;
                case SourceType.Twitch:
                    maxChars = 500;
                    break;
                case SourceType.Discord:
                    maxChars = IsPrivate ? 4000 : (4000 - Promt.Length - 50);
                    break;
            }

            string result = @$"
            Datum/Zeit: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
            Nutzer: {Name}
            ID: {Id}
            Privates gespräch: {IsPrivate}
            Quelle: {Source}
            Channel: {Channel}
            Promt: {Promt}
            max. Zeichen: {maxChars}
            ";
            return result;
        }
#pragma warning restore CS0114 

        public enum SourceType
        {
            Twitch,
            Discord,
            Console
        }
    }    
    #endregion
}
