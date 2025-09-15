using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Types;
using SophBot.bot.conf;
using SophBot.bot.database;

namespace SophBot.bot.ai
{
    public class SGeminiEngine
    {
#pragma warning disable CS8618
        private static ChatSession Session;
        private static GoogleAi GoogleAI;
#pragma warning restore CS8618

        public static async Task StartSession()
        {
            GoogleAI = new GoogleAi(SConfig.AI.GeminiKey);

            var model = GoogleAI.CreateGenerativeModel("models/gemini-2.5-flash");

            ThinkingConfig thinkConf = new ThinkingConfig
            {
                ThinkingBudget = 50
            };

            GenerationConfig genConf = new GenerationConfig
            {
                ThinkingConfig = thinkConf,
                Temperature = 1
            };

            string SystemInstructions = SConfig.AI.SystemInstructions;

            if (SConfig.AI.UseWiki)
            {
                var dbValues = await SDBEngine.SelectFromAsync("wiki", ["title", "site", "value"]);
                string[] wiki = new string[dbValues.Length + 1];
                wiki[0] = "wiki format: {title} (site): {content}";
                for (int i = 1; i <= dbValues.Length; i++)
                {
                    wiki[i] = $"{dbValues[i - 1][0]} {dbValues[i - 1][1]}: {dbValues[i - 1][2]}";
                }
                SystemInstructions += " WIKI INFORMATION: \n" + string.Join("\n", wiki);
            }

            model.UseGoogleSearch = true;
            model.EnableFunctions();

            // Register WriteMemory function
            model.AddFunctionTool(
                new FunctionTool(
                    name: "WriteMemory",
                    description: "Speichert einen Text in die interne Memory-Datei.",
                    parameters: new FunctionParameters
                    {
                        Properties = new Dictionary<string, FunctionProperty>
                        {
                            { "content", new FunctionProperty(FunctionType.String, "Inhalt, der gespeichert werden soll.") }
                        },
                        Required = new[] { "content" }
                    },
                    function: async (args) =>
                    {
                        // args contains the function call arguments from the model
                        var content = args.ContainsKey("content") ? args["content"]?.ToString() ?? string.Empty : string.Empty;
                        Memory.WriteMemory(content);
                        return "Memory updated.";
                    }
                )
            );

            // Register ReadMemory function
            model.AddFunctionTool(
                new FunctionTool(
                    name: "ReadMemory",
                    description: "Liest den aktuell gespeicherten Text aus der Memory-Datei.",
                    parameters: new FunctionParameters
                    {
                        Properties = new Dictionary<string, FunctionProperty>()
                    },
                    function: async (args) =>
                    {
                        return Memory.ReadMemory();
                    }
                )
            );

            Session = model.StartChat(config: genConf, systemInstruction: SystemInstructions);
        }

        public static async ValueTask<string> GenerateResponseAsync(AIRequest request)
        {
            var response = await Session.GenerateContentAsync(request.ToString());

            // If the model chose to call a function, execute it automatically
            if (response.HasFunctionCall)
            {
                // Executes the registered function and returns its result
                var funcResult = await response.CallFunctionAsync();

                // Option A: return the function result directly to the caller
                if (funcResult != null)
                    return funcResult.ToString()!;

                // Option B (alternative): feed the function result back into the model so it can produce a follow-up message
                // var followUp = await Session.GenerateContentAsync(funcResult?.ToString() ?? string.Empty);
                // if (followUp.Text != null) return followUp.Text;

                return "Function returned no result.";
            }

            if (response.Text is null)
            {
                response = await Session.GenerateContentAsync(request.ToString());
                if (response.Text is null)
                    return "KI überladen! Bitte später erneut versuchen!";
            }

            if (response.Text.Length > request.CharLimit)
                response = await Session.GenerateContentAsync("SYSTEM > Du hast das maximale Zeichenlimit überschritten, weshalb die Nachricht noch nicht gesendet wurde. Bitte erneut generieren!");

            if (response.Text is null)
                return "KI überladen! Bitte später erneut versuchen!";

            return response.Text;
        }

        internal static class Memory
        {
            private static string FileLocation = $"{AppDomain.CurrentDomain.BaseDirectory}/aiMemory/memory.txt";

            public static void WriteMemory(string content)
            {
                var dir = Path.GetDirectoryName(FileLocation);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(FileLocation))
                    File.Create(FileLocation).Dispose();

                File.WriteAllText(FileLocation, content);
            }

            public static string ReadMemory()
            {
                return File.Exists(FileLocation) ? File.ReadAllText(FileLocation) : "Memory Empty!";
            }
        }
    }
}