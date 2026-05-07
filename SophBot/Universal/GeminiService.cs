using DSharpPlus.Commands;
using DSharpPlus.Entities;
using GenerativeAI;
using GenerativeAI.Tools;
using Microsoft.Extensions.Hosting;
using System.Data;
using TwitchSharp.Api.Clients;

namespace SophBot.Universal
{
    public class GeminiService : IHostedService
    {
#pragma warning disable CS8618
        private GoogleAi googleAI { get; set; }
        private GenerativeModel mainModel { get; set; }
        private GenerativeModel googleModel { get; set; }
        private ChatSession mainChat { get; set; }
        private ChatSession googleChat { get; set; }
#pragma warning restore CS8618


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            googleAI = new GoogleAi(Environment.GetEnvironmentVariable("Gemini.Token") ?? throw new Exception("Gemini Token was not found in appsettings!"));
            mainModel = googleAI.CreateGenerativeModel("models/gemini-3-flash-preview"); 
            googleModel = googleAI.CreateGenerativeModel("models/gemini-2.5-flash");


            mainModel.UseGoogleSearch = false;
            mainModel.SystemInstruction = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/ai/prompt.txt");

            mainModel.EnableFunctions();
            //MainModel.AddFunctionTool(new QuickTool((string? filter = null) => ReadMemory(filter), "ReadMemory", "Lies die gespeicherten Informationen (mit dem optionalen filter kannst du nur die Einträge anzeigen, die genau diesen string enthalten)"));
            //MainModel.AddFunctionTool(new QuickTool((string content) => WriteMemory(content), "WriteMemory", "Speichere eine neue Information"));
            //MainModel.AddFunctionTool(new QuickTool((long id, string newContent) => ModifyMemory(id, newContent), "ModifyMemory", "Bearbeite eine Information anhand der id (readMemory)"));
            //MainModel.AddFunctionTool(new QuickTool((long id) => DeleteMemory(id), "DeleteMemory", "Lösche eine Information anhand der id (readMemory)"));
            mainModel.AddFunctionTool(new QuickTool((long userId, string noteContent) => AddUserAiNote(userId, noteContent), "AddUserAiNote", "Notiere dir eine Information zu dem aktuellen Nutzer"));
            mainModel.AddFunctionTool(new QuickTool((long userId, int arrayIndex, string noteContent) => ModifyUserAiNote(userId, arrayIndex, noteContent), "ModifyUserAiNote", "Notiere dir eine Information zu dem aktuellen Nutzer"));
            mainModel.AddFunctionTool(new QuickTool((string? filter = null) => ReadWiki(filter), "ReadWiki", "Erhalte die Informationen des Internen Soph-Wikis"));
            //MainModel.AddFunctionTool(new QuickTool((long id) => GetProfile(id), "GetProfile", "Erhalte mehr Informationen über einen Benutzer"));
            //MainModel.AddFunctionTool(new QuickTool(async() => await GetCurrentStream(), "GetCurrentStream", "Erhalte den Informationen über den aktuell Laufenden Stream"));
            mainModel.AddFunctionTool(new QuickTool((string request) => AskGoogleAi(request), "AskGoogleAi", "Frage ein KI-Modell, mit der möglichkeit google zu durchsuchen, nach Informationen"));


            googleModel.UseGoogleSearch = true;

            StartNewmainChat();
            StartNewgoogleChat();

            googleChat.DisableFunctions();
            return Task.CompletedTask;
        }

        public void StartNewmainChat()
        {
            mainChat = mainModel.StartChat();
        }
        public void StartNewgoogleChat()
        {
            googleChat = googleModel.StartChat();
        }

        public async Task<string> GenerateResponseAsync(BaseAiRequest request)
        {
            try
            {
                return (await mainChat.GenerateContentAsync(request.ToString())).Text;
            } catch (Exception ex)
            {
                if (ex.Message.Contains("The model is overloaded")) return "System Überladen! - Bitte später erneut versuchen!";
                return $"Unknown Error: {ex.Message}";
            }
            
        }

        #region Function Tools
        private string AddUserAiNote(long userId, string noteContent)
        {
            Profile profile = new Profile(userId);
            profile.AddAiNote(noteContent);
            return $"Added Note! New Profile: {profile.AsAiString()}";
        }
        private string ModifyUserAiNote(long userId, int arrayIndex, string noteContent)
        {
            Profile profile = new Profile(userId);
            profile.SetAiNote(arrayIndex, noteContent);
            return $"Modified Note! New Profile: {profile.AsAiString()}";
        }
        
        private string ReadWiki(string? filter = null)
        {
            string result = string.Empty;
            DataTable data = Program.GetService<DatabaseService>().SelectTable(DBTable.Wiki, "article");
            
            result += string.Join(" | ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName)) + "\n";
            foreach(DataRow row in data.Rows)
            {
                if (filter is null || row.ItemArray.Contains(filter)) result += string.Join(" | ", row.ItemArray) + "\n";
            }
            return result;
        }

        /*private async Task<string> GetCurrentStream()
        {
            UserData user = (await TwitchEngine.Client.Users.GetUsersAsync(logins: ["xsophe"]))[0];
            var stream = await TwitchEngine.Client.Streams.GetFollowedStreamsAsync(user.Id);
            if (stream is null)
                return "Null - No Stream active!";
            return $@"Titel: {stream}
Kategorie: {stream.GameName}
Gestartet um: {stream.StartedAt.ToString("dd.MM.yyyy - HH:mm:ss")}
Aktuelle Zuschauer: {stream.CurrentViewer}";
        }*/
        
        private string AskGoogleAi(string request)
        {
            try
            {
                return googleChat.GenerateContentAsync(request).Result.Text;
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
        public readonly string Prompt;
        public readonly AiRequestType Source;

        public BaseAiRequest(AiRequestType source, string name, long id, string prompt)
        {
            Source = source;
            Name = name;
            Id = id;
            Prompt = prompt;
        }

        public abstract override string ToString();
    }
    public class DiscordAiRequest : BaseAiRequest
    {
        public readonly DiscordChannel Channel;
        public readonly bool IsPrivate;
        public readonly Profile Profile;
        public DiscordAiRequest(DiscordChannel channel, DiscordUser sender, string prompt)
            : base(AiRequestType.DISCORD, sender.GlobalName ?? sender.Username, 0 , prompt)
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
userPromt: {Prompt}";
        }
    }
    public class TwitchAiRequest : BaseAiRequest
    {
        public readonly string Channel;
        public readonly bool IsPrivate;
        public readonly Profile Profile;
        public TwitchAiRequest(string channel, bool isPrivateChat, UserData sender, string prompt)
            : base(AiRequestType.TWITCH, sender.DisplayName, 0 , prompt)
        {
            Channel = channel;
            IsPrivate = isPrivateChat;
            Profile = new Profile(sender.Id);
            Id = Profile.ID;
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
channelname: {Channel}
isPrivate: {IsPrivate}
userInformation: {Profile.AsAiString()}
userPromt: {Prompt}";
        }
    }
    public class ConsoleAiRequest : BaseAiRequest
    {
        public ConsoleAiRequest(string prompt)
            : base(AiRequestType.CONSOLE, "CONSOLE", -100 , prompt)
        {
            
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
consolePromt: {Prompt}";
        }
    }
    public class SystemAiRequest : BaseAiRequest
    {
        public SystemAiRequest(string prompt)
            : base(AiRequestType.SYSTEM, "SYSTEM", -101 , prompt)
        {
            
        }

        public override string ToString()
        {
            return $@"source: {Source}
dateTime: {DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")}
systemPromt: {Prompt}";
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
