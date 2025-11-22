using SophBot.Universal;
using TwitchSharp;
using TwitchSharp.Events.Types;

namespace SophBot.Twitch.EventHandlers
{
    public static class ClientWhisperReceivedHandler
    {
        public static async Task OnReceive (TwitchClient s, ClientWhisperReceivedArgs e)
        {
            if (e.Sender == s.CurrentUser) return;

            string response = await GeminiEngine.GenerateResponseAsync(new TwitchAiRequest($"private({e.Sender.DisplayName})", true, e.Sender, e.MessageContent));
            await e.Sender.SendWhisperAsync(response);
        }
    }
}