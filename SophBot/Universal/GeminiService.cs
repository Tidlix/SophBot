using DSharpPlus.Commands;
using DSharpPlus.Entities;
using GenerativeAI;
using GenerativeAI.Tools;
using Microsoft.Extensions.Hosting;
using System.Data;

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
        private TextFileEditor SIEditor { get; set; }
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
            SIEditor = new TextFileEditor($"{AppDomain.CurrentDomain.BaseDirectory}/ai/dynamicSystemInstructions.txt");
            mainModel.SystemInstruction = ReadFullSystemInstruction();

            mainModel.EnableFunctions();
            mainModel.AddFunctionTool(new QuickTool(AddLineToSystemInstructions, "AddLineToSystemInstructions", "Füge eine neue Zeile zu den Systemanweisungen hinzu"));
            mainModel.AddFunctionTool(new QuickTool(AddLinesToSystemInstructions, "AddLinesToSystemInstructions", "Füge mehrere Zeilen zu den Systemanweisungen hinzu"));
            mainModel.AddFunctionTool(new QuickTool(SetLineFromSystemInstructions, "SetLineFromSystemInstructions", "Setzt den Text einer vorhandenen Zeile der Systemanweisungen"));
            mainModel.AddFunctionTool(new QuickTool(RemoveLineFromSystemInstructions, "RemoveLineFromSystemInstructions", "Entfernt eine Zeile aus den Systemanweisungen"));
            mainModel.AddFunctionTool(new QuickTool(ReadWiki, "ReadWiki", "Erhalte die Informationen des Internen Soph-Wikis"));
            mainModel.AddFunctionTool(new QuickTool(AskGoogleAi, "AskGoogleAi", "Frage ein KI-Modell, mit der möglichkeit google zu durchsuchen, nach Informationen"));


            googleModel.UseGoogleSearch = true;

            StartNewmainChat();
            StartNewgoogleChat();

            googleChat.DisableFunctions();
            return Task.CompletedTask;
        }

        private string ReadFullSystemInstruction()
        {
            return ReadStaticSystemInstruction() + "\n\n" + ReadDynamicSystemInstruction();
        }
        private string ReadStaticSystemInstruction()
        {
            return $@"---Start Statische Systemanweisungen---
{File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/ai/staticSystemInstructions.txt")} 
---Ende Statische Systemanweisung---";
        }
        private string ReadDynamicSystemInstruction()
        {
            string[] lines = SIEditor.ReadAllLines();
            string text = string.Empty;
            int index = 0;
            foreach(string line in lines)
            {
                text += $"[{index}] {line}\n";
                index++;
            }
            return $@"---Start Dynamische Systemanweisungen---
{text} 
---Ende Dynamische Systemanweisung---";
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
        private string AddLineToSystemInstructions(string text)
        {
            SIEditor.AppendNewLine(text);
            return ReadFullSystemInstruction();
        }
        private string AddLinesToSystemInstructions(string[] texts)
        {
            SIEditor.AppendNewLines(texts);
            return ReadFullSystemInstruction();
        }
        private string SetLineFromSystemInstructions(int lineIndex, string text)
        {
            SIEditor.SetLineText(lineIndex, text);
            return ReadFullSystemInstruction();
        }
        private string RemoveLineFromSystemInstructions(int lineIndex)
        {
            SIEditor.DeleteLine(lineIndex);
            return ReadFullSystemInstruction();
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
    /*public class TwitchAiRequest : BaseAiRequest
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
    }*/
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
