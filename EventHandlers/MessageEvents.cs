using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Configuration;
using SophBot.Objects;
using System.Text.RegularExpressions;
using TwitchLib.Api.Core.Extensions.System;

namespace SophBot.EventHandlers
{
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

            TDiscordGuild guild = new(e.Guild);
            string? output = await guild.getCommandAsnyc(command);

            if (output == null)
            {
                return;
            }

            if (output.Contains("[user]")) output = output.Replace("[user]", e.Author.Mention);
            if (output.Contains("[text]")) output = output.Replace("[text]", text);

            string wordPattern = @"\[word\((\d+)\)\]";
            output = Regex.Replace(output, wordPattern, match =>
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
            output = Regex.Replace(output, randPattern, match =>
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

            await e.Message.RespondAsync(output);
        }

        private async ValueTask aiRequestAsync(DiscordClient s, MessageCreatedEventArgs e)
                {
                    try
                    {
                        string promt = e.Message.Content.Substring(e.Message.Content.IndexOf(s.CurrentUser.Mention) + s.CurrentUser.Mention.Length + 1);
                        string response = await TSophBotAI.generateResponse(promt);

                        await e.Message.RespondAsync(response);
                    }
                    catch (Exception ex)
                    {
                        TLog.sendLog(ex.Message, TLog.MessageType.Error);
                    }
                }

        private async ValueTask addPointsAsnyc(MessageCreatedEventArgs e)
        {
            DiscordMember m = await e.Guild.GetMemberAsync(e.Author.Id);
            TDiscordMember member = new(m);

            ulong points = (ulong)((m.PremiumType.HasValue) ? 10 : 5);

            await member.addGuildPointsAsnyc((points));
        }
    }
}