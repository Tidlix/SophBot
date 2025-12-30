using SophBot.Universal;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace SophBot.Twitch.Commands
{
    public class AICommands
    {
        public static async Task HandleCommandAsync(object? s, OnChatCommandReceivedArgs args)
        {
            var channel = await TwitchEngine.TwitchSharpClient.GetUserByLoginAsync(args.ChatMessage.Channel.ToLower());
            var sender = await TwitchEngine.TwitchSharpClient.GetUserByIDAsync(args.ChatMessage.UserId);
            string response;

            if (args.Command.ArgumentsAsList.Count == 0)
                response = "Um das AI Feature zu nutzen, musst du deine Anfrage hinter den Command schreiben! (z.B. !ai Hallo, wie geht es dir)";
            else
                response = await GeminiEngine.GenerateResponseAsync(new TwitchAiRequest(
                    channel: channel.DisplayName, 
                    isPrivateChat: false, 
                    sender: sender, 
                    promt: args.Command.ArgumentsAsString));
            

            await channel.SendChatMessageAsync(response, args.ChatMessage.Id);
        }
    }
}