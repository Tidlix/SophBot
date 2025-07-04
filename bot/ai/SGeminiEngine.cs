using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using SophBot.bot.conf;
using SophBot.bot.logs;

namespace SophBot.bot.ai
{
    public class SGeminiEngine
    {
#pragma warning disable CS8618
        private static ChatSession Session;
        private static GoogleAi GoogleAI;
#pragma warning restore CS8618

        public static void StartSession()
        {
            GoogleAI = new GoogleAi(SConfig.AI.GeminiKey);

            var model = GoogleAI.CreateGenerativeModel("models/gemini-2.5-flash");

            ThinkingConfig thinkConf = new ThinkingConfig
            {
                ThinkingBudget = 0
            };

            GenerationConfig genConf = new GenerationConfig
            {
                ThinkingConfig = thinkConf
            };

            Session = model.StartChat(config: genConf, systemInstruction: SConfig.AI.SystemInstructions);
        }

        public static async ValueTask<string> GenerateResponseAsync(string prompt)
        {
            var response = await Session.GenerateContentAsync(prompt);
            return response.Text ?? string.Empty;
        }
    }
}
