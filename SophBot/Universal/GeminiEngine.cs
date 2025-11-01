using DSharpPlus.Entities;
using TwitchSharp.Entitys;
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

            /*
            To Do Functions:
            - google
            - readLastMessages
            - recode memory (see below)
            */

            GoogleModel.UseGoogleSearch = true;

            StartNewMainChat();
            StartNewGoogleChat();

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
    }
    public class AiRequest
    {
        public SourceType Source;
        public string Name { get; private set; }
        public string Id { get; private set; }
        public string Promt { get; private set; }
        public bool IsPrivate { get; private set; }

        public AiRequest(DiscordUser user, string promt, bool isPrivate)
        {
            Source = SourceType.Discord;
            Id = user.Id.ToString();
            Name = user.GlobalName;
            Promt = promt;
            IsPrivate = isPrivate;
        }
        public AiRequest(TwitchUser user, string promt, bool isPrivate)
        {
            Source = SourceType.Twitch;
            Id = user.ID;
            Name = user.DisplayName;
            Promt = promt;
            IsPrivate = isPrivate;
        }
        public AiRequest(string promt, bool isConsole = false)
        {
            Source = SourceType.Console;
            Id = isConsole ? "0" : "-1";
            Name = isConsole ? "CONSOLE" : "SYSTEM";
            Promt = promt;
            IsPrivate = true;
        }

#pragma warning disable CS0114
        public string ToString()
        {
            // [DateTime] {Name} (id={id}) schreibt über {Plattform} {Optional: (Im Privaten)}: {Promt}
            return $"[{DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}] {Name} (id={Id}) schreibt über {Source}{(IsPrivate ? "( Im Privaten)" : "")}: {Promt}";
        }
#pragma warning restore CS0114 

        public enum SourceType
        {
            Twitch,
            Discord,
            Console
        }
    }    
}
