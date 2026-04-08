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
#pragma warning restore CS8618

        public static void Initialize(string token)
        {
            GoogleAI = new GoogleAi(token);
            MainModel = GoogleAI.CreateGenerativeModel("models/gemini-3-flash-preview"); 
            GoogleModel = GoogleAI.CreateGenerativeModel("models/gemini-2.5-flash");


            MainModel.UseGoogleSearch = false;
            MainModel.SystemInstruction = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/ai/promt.txt");

            MainModel.EnableFunctions();
            //MainModel.AddFunctionTool(new QuickTool((string? filter = null) => ReadMemory(filter), "ReadMemory", "Lies die gespeicherten Informationen (mit dem optionalen filter kannst du nur die Einträge anzeigen, die genau diesen string enthalten)"));
            //MainModel.AddFunctionTool(new QuickTool((string content) => WriteMemory(content), "WriteMemory", "Speichere eine neue Information"));
            //MainModel.AddFunctionTool(new QuickTool((long id, string newContent) => ModifyMemory(id, newContent), "ModifyMemory", "Bearbeite eine Information anhand der id (readMemory)"));
            //MainModel.AddFunctionTool(new QuickTool((long id) => DeleteMemory(id), "DeleteMemory", "Lösche eine Information anhand der id (readMemory)"));
            MainModel.AddFunctionTool(new QuickTool((long userId, string noteContent) => AddUserAiNote(userId, noteContent), "AddUserAiNote", "Notiere dir eine Information zu dem aktuellen Nutzer"));
            MainModel.AddFunctionTool(new QuickTool((long userId, int arrayIndex, string noteContent) => ModifyUserAiNote(userId, arrayIndex, noteContent), "ModifyUserAiNote", "Notiere dir eine Information zu dem aktuellen Nutzer"));
            MainModel.AddFunctionTool(new QuickTool(() => ReadWiki(), "ReadWiki", "Erhalte die Informationen des Internen Soph-Wikis"));
            //MainModel.AddFunctionTool(new QuickTool((long id) => GetProfile(id), "GetProfile", "Erhalte mehr Informationen über einen Benutzer"));
            //MainModel.AddFunctionTool(new QuickTool(async() => await GetCurrentStream(), "GetCurrentStream", "Erhalte den Informationen über den aktuell Laufenden Stream"));
            MainModel.AddFunctionTool(new QuickTool((string request) => AskGoogleAi(request), "AskGoogleAi", "Frage ein KI-Modell, mit der möglichkeit google zu durchsuchen, nach Informationen"));


            GoogleModel.UseGoogleSearch = true;

            StartNewMainChat();
            StartNewGoogleChat();

            GoogleChat.DisableFunctions();

            //GenerateResponseAsync(new SystemAiRequest("liese deine gespeicherten Informationen - Gib diese nicht aus und warte auf weitere Anfragen")).Wait();
        }

        public static void StartNewMainChat()
        {
            MainChat = MainModel.StartChat();
        }
        public static void StartNewGoogleChat()
        {
            GoogleChat = GoogleModel.StartChat();
        }

        public static async Task<string> GenerateResponseAsync(BaseAiRequest request)
        {
            try
            {
                return (await MainChat.GenerateContentAsync(request.ToString())).Text;
            } catch (Exception ex)
            {
                if (ex.Message.Contains("The model is overloaded")) return "System Überladen! - Bitte später erneut versuchen!";
                return $"Unknown Error: {ex.Message}";
            }
            
        }

        #region Function Tools
        private static string AddUserAiNote(long userId, string noteContent)
        {
            Profile profile = new Profile(userId);
            profile.AddAiNote(noteContent);
            return $"Added Note! New Profile: {profile.AsAiString()}";
        }
        private static string ModifyUserAiNote(long userId, int arrayIndex, string noteContent)
        {
            Profile profile = new Profile(userId);
            profile.SetAiNote(arrayIndex, noteContent);
            return $"Modified Note! New Profile: {profile.AsAiString()}";
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

        /*private static async Task<string> GetCurrentStream()
        {
            TwitchUser user = await TwitchEngine.TwitchSharpClient.GetUserByLoginAsync("xsophe");
            TwitchStream? stream = user.GetCurrentStream();
            if (stream is null)
                return "Null - No Stream active!";
            return $@"Titel: {stream.Title}
Kategorie: {stream.GameName}
Gestartet um: {stream.StartedAt.ToString("dd.MM.yyyy - HH:mm:ss")}
Aktuelle Zuschauer: {stream.CurrentViewer}";
        }
        */ 
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
    public abstract class BaseAiRequest
    {
        public readonly string Name;
        public long Id { get; init; }
        public readonly string Promt;
        public readonly AiRequestType Source;

        public BaseAiRequest(AiRequestType source, string name, long id, string promt)
        {
            Source = source;
            Name = name;
            Id = id;
            Promt = promt;
        }

        public abstract override string ToString();
    }
    public class DiscordAiRequest : BaseAiRequest
    {
        public readonly DiscordChannel Channel;
        public readonly bool IsPrivate;
        public readonly Profile Profile;
        public DiscordAiRequest(DiscordChannel channel, DiscordUser sender, string promt)
            : base(AiRequestType.DISCORD, sender.GlobalName ?? sender.Username, 0 , promt)
        {
            Channel = channel;
            IsPrivate = channel.IsPrivate;
            Profile = new Profile(sender.Id);
            Id = Profile.ID;
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
channelname (id): {Channel.Name} ({Channel.Id})
isPrivate: {IsPrivate}
userInformation: {Profile.AsAiString()}
userPromt: {Promt}";
        }
    }
    /*public class TwitchAiRequest : BaseAiRequest
    {
        public readonly string Channel;
        public readonly bool IsPrivate;
        public readonly Profile Profile;
        public TwitchAiRequest(string channel, bool isPrivateChat, TwitchUser sender, string promt)
            : base(AiRequestType.TWITCH, sender.DisplayName, 0 , promt)
        {
            Channel = channel;
            IsPrivate = isPrivateChat;
            Profile = new Profile(sender.ID);
            Id = Profile.ID;
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
channelname: {Channel}
isPrivate: {IsPrivate}
userInformation: {Profile.AsAiString()}
userPromt: {Promt}";
        }
    }*/
    public class ConsoleAiRequest : BaseAiRequest
    {
        public ConsoleAiRequest(string promt)
            : base(AiRequestType.CONSOLE, "CONSOLE", -100 , promt)
        {
            
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
consolePromt: {Promt}";
        }
    }
    public class SystemAiRequest : BaseAiRequest
    {
        public SystemAiRequest(string promt)
            : base(AiRequestType.SYSTEM, "SYSTEM", -101 , promt)
        {
            
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
systemPromt: {Promt}";
        }
    }

    public enum AiRequestType
    {
        DISCORD,
        TWITCH,
        CONSOLE,
        SYSTEM
    }
    #endregion
}
