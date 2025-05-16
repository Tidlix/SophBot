using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Configuration;
using SophBot.Database;
using SophBot.Messages;
using System.Text.RegularExpressions;
using TwitchLib.Api.Core.Extensions.System;

namespace SophBot.EventHandlers {
    class MessageEvents : IEventHandler<MessageCreatedEventArgs>
    {
        private static readonly Dictionary<ulong, int> messageCount = new();

        public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            if (e.Author.Equals(s.CurrentUser)) return;

            ulong userId = e.Author.Id;

            if (!messageCount.ContainsKey(userId)) messageCount.Add(userId, 1);
            else if (messageCount[userId] == 10)
            {
                messageCount.Remove(userId);
                await addPointsAsnyc(e);
            }
            else
            {
                messageCount[userId] += 1;
            }

            if (e.Message.Content.Contains(s.CurrentUser.Mention))
            {
                await aiRequestAsync(s, e);
            }
            if (e.Message.Content.StartsWith("!") && !e.Message.Content.Equals("!"))
            {
                await customCommandAsync(e);
            }
        }

        private async ValueTask customCommandAsync(MessageCreatedEventArgs e)
        {
            string msg = e.Message.Content.Substring(e.Message.Content.IndexOf('!') + 1);
            string command = (msg.Contains(' ')) ? msg.Substring(0, msg.IndexOf(' ')).ToLower() : msg.ToLower();
            string text = (msg.Contains(' ')) ? msg.Substring(msg.IndexOf(' ')) : "";
            Random random = new Random();

            try
            {
                if (!await TidlixDB.CustomCommands.checkExistanceAsync(command, e.Guild.Id))
                {
                    return;
                }
                string response = await TidlixDB.CustomCommands.getCommandAsnyc(command, e.Guild.Id);

                if (response.Contains("[user]")) response = response.Replace("[user]", e.Author.Mention);
                if (response.Contains("[text]")) response = response.Replace("[text]", text);

                string wordPattern = @"\[word\((\d+)\)\]";
                response = Regex.Replace(response, wordPattern, match =>
                {
                    int word = int.Parse(match.Groups[1].Value);
                    string[] words = text.Split(' ');
                    try
                    {
                        return words[word];
                    }
                    catch
                    {
                        return "";
                    }

                });

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




                await e.Message.RespondAsync(response);

            }
            catch (Exception ex)
            {
                await Log.sendMessage($"Failed to try customcommand - {ex.Message}", MessageType.Warning());
            }
        }

        private async ValueTask aiRequestAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            try
            {
                string promt = e.Message.Content.Substring(e.Message.Content.IndexOf(s.CurrentUser.Mention) + s.CurrentUser.Mention.Length + 1);
                string response = await Services.AI.Generate(promt);

                await e.Message.RespondAsync(response);
            }
            catch (Exception ex)
            {
                await Log.sendMessage(ex.Message, MessageType.Error());
            }
        }

        private async ValueTask addPointsAsnyc(MessageCreatedEventArgs e)
        {
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            
            var points = await TidlixDB.UserProfiles.getPointsAsync(e.Guild.Id, e.Author.Id);
            await TidlixDB.UserProfiles.modifyValueAsnyc("points", (points + ((member.PremiumType.HasValue) ? 5 : 10)).ToString(), e.Guild.Id, e.Author.Id);
        }
    }
}