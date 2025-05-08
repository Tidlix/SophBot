using DSharpPlus;
using DSharpPlus.EventArgs;
using SophBot.Database;
using SophBot.Messages;
using System.Text.RegularExpressions;
using TwitchLib.Api.Helix;

namespace SophBot.EventHandlers {
    class MessageEvents : IEventHandler<MessageCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            if (e.Message.Content.StartsWith("!"))
            {
                await customCommand(e);
            }
        }


        public async ValueTask customCommand(MessageCreatedEventArgs e) {
            string msg = e.Message.Content.Substring(e.Message.Content.IndexOf('!')+1);
            string command = (msg.Contains(' ')) ? msg.Substring(0, msg.IndexOf(' ')).ToLower() : msg.ToLower();
            string text = (msg.Contains(' ')) ? msg.Substring(msg.IndexOf(' ')) : "";
            Random random = new Random();

            try {
                if (!await TidlixDB.checkCommandExists(command, e.Guild.Id)){
                    return;
                }
                string response = await TidlixDB.getCommand(command, e.Guild.Id);

                string randPattern = @"\[rand\((?:(\d+),)?(\d+)\)\]";
                response = Regex.Replace(response, randPattern, match =>
                {
                    int min = (match.Groups[1].Success) ? int.Parse(match.Groups[1].Value) : 0;
                    int max = int.Parse(match.Groups[2].Value);

                    if (min > max)
                    {
                        return match.Value; 
                    }

                    int zufallswert = random.Next(min, max + 1);

                    return zufallswert.ToString();
                });

                string wordPattern = @"\[word\((\d+)\)\]";
                response = Regex.Replace(response, wordPattern, match =>
                {
                    int word = int.Parse(match.Groups[1].Value);
                    string[] words = text.Split(' ');
                    try
                    {
                        return words[word];
                    } catch 
                    {
                        return "";
                    }
                    
                });


                if (response.Contains("[user]")) response = response.Replace("[user]", e.Author.Mention);
                if (response.Contains("[text]")) response = response.Replace("[text]", text);

                await e.Message.RespondAsync(response);

            } catch (Exception ex) {
                await MessageSystem.sendMessage($"Failed to try customcommand - {ex.Message}", MessageType.Warning());
            }
        }
    }
}