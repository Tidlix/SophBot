using GenerativeAI;
using GenerativeAI.Types;
using GenerativeAI.Tools;
using SophBot.bot.conf;
using SophBot.bot.database;
using System.ComponentModel;
using CSharpToJsonSchema;

namespace SophBot.bot.ai
{
    public class SGeminiEngine
    {
#pragma warning disable CS8618
        private static ChatSession Session;
        private static GoogleAi GoogleAI;
#pragma warning restore CS8618

        private static string MemoryLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aiMemory", "memory.txt");

        public static async Task StartSession()
        {
            GoogleAI = new GoogleAi(SConfig.AI.GeminiKey);
            var model = GoogleAI.CreateGenerativeModel("models/gemini-2.5-flash");

            var thinkConf = new ThinkingConfig { ThinkingBudget = 50 };
            var genConf = new GenerationConfig { ThinkingConfig = thinkConf, Temperature = 1 };

            string systemInstructions = SConfig.AI.SystemInstructions;
            if (SConfig.AI.UseWiki)
            {
                var dbValues = await SDBEngine.SelectFromAsync("wiki", new[] { "title", "site", "value" });
                string[] wiki = new string[dbValues.Length + 1];
                wiki[0] = "wiki format: {title} (site): {content}";
                for (int i = 1; i <= dbValues.Length; i++)
                    wiki[i] = $"{dbValues[i - 1][0]} {dbValues[i - 1][1]}: {dbValues[i - 1][2]}";
                systemInstructions += " WIKI INFORMATION: \n" + string.Join("\n", wiki);
            }

            //model.UseGoogleSearch = true;
            //model.EnableFunctions();
            /*
            var readTool = new QuickTool(() =>
            {
                return ReadMemory();
            }, "ReadMemory", "Read your saved Memory");

            var writeTool = new QuickTool(([Description("Memory content")] string content) =>
            {
                WriteMemory(content);
            }, "WriteMemory", "Rewrite all of your saved memory - DELETES ALL NOT PROVIDED CONTENT!");

            model.AddFunctionTool(readTool);
            model.AddFunctionTool(writeTool);
            */

            Session = model.StartChat(config: genConf, systemInstruction: systemInstructions);
        }

        [FunctionTool(GoogleFunctionTool = true)]
        [Description("Rewrite all of your saved memory - DELETES ALL NOT PROVIDED CONTENT!")]
        public static void WriteMemory(string content)
        {
            var dir = Path.GetDirectoryName(MemoryLocation);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(MemoryLocation))
                File.Create(MemoryLocation).Dispose();
            File.WriteAllText(MemoryLocation, content);
        }

        [FunctionTool(GoogleFunctionTool = true)]
        [Description("Read your Memory")]
        public static string ReadMemory()
        {
            return File.Exists(MemoryLocation) ? File.ReadAllText(MemoryLocation) : "Memory Empty!";
        }

        public static async ValueTask<string> GenerateResponseAsync(AIRequest request)
        {
            /*
            Function calling will happen here later
            */
            var response = await Session.GenerateContentAsync(request.ToString());
            
            if (response.Text is null)
            {
                response = await Session.GenerateContentAsync(request.ToString());
                if (response.Text is null) return "KI überladen! Bitte später erneut versuchen!";
            }
            if (response.Text.Length > request.CharLimit)
                response = await Session.GenerateContentAsync("SYSTEM > Du hast das maximale Zeichenlimit überschritten, weshalb die Nachricht noch nicht gesendet wurde. Bitte erneut generieren!");
            if (response.Text is null)
                return "KI überladen! Bitte später erneut versuchen!";
            return response.Text;
        }
    }
}