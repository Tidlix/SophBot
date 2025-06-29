using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class CCEvents :
        IEventHandler<MessageCreatedEventArgs>,
        IEventHandler<ModalSubmittedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
        {
            string msg = e.Message.Content;
            if (!msg.StartsWith('!')) return;

            SLogger.Log(LogLevel.Debug, "Found custom command request", "CCEvents.cs");


            try
            {
                string cmd = (msg.Contains(' ')) ? msg.Substring(1, msg.IndexOf(' ')-1).ToLower() : msg.Substring(1);

                SLogger.Log(LogLevel.Debug, $"Got cmd {cmd}", "CCEvents.cs");
                string text = (msg.Contains(' ')) ? msg.Substring(msg.IndexOf(' ')+1) : "";
                SLogger.Log(LogLevel.Debug, $"Got parameters {text}", "CCEvents.cs");
                Random random = new Random();

                SDiscordServer server = new(e.Guild);
                SLogger.Log(LogLevel.Debug, "Try to get command output", "CCEvents.cs");
                string? output = await server.Commands.getOutputAsync(cmd);


                if (output == "")
                {
                    return;
                }
                SLogger.Log(LogLevel.Debug, $"Got command output {output}", "CCEvents.cs");

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

                    int rand = random.Next(min, max + 1);

                    return rand.ToString();
                });

                SLogger.Log(LogLevel.Debug, "Sending command response", "CCEvents.cs");
                await e.Message.RespondAsync(output);
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Something went wrong while executing custom command", "CCEvents.cs", ex);
            }
            
        }

        public async Task HandleEventAsync(DiscordClient s, ModalSubmittedEventArgs e)
        {
            if (!e.Id.Contains("ccSystem")) return;

            await e.Interaction.DeferAsync(true);
            SLogger.Log(LogLevel.Debug, "Got command modal interaction", "CCEvents.cs");

            string pattern = @"cmd=([^}]+);";
            string command; 
    


            Match match = Regex.Match(e.Id, pattern);
            if (match.Success)
            {
                command = match.Groups[1].Value;
                SLogger.Log(LogLevel.Debug, $"Got command {command}", "CCEvents.cs");
            }
            else
            {
                throw new Exception("Coudln't find match in customID!");
            }

            string value = e.Values.Values.First();
            SDiscordServer server = new(e.Interaction.Guild!);
            try
            {
                if (!(await server.Commands.getOutputAsync(command) == ""))
                {
                    if (value == "")
                    {
                        SLogger.Log(LogLevel.Debug, "Deleting command", "CCEvents.cs");
                        await server.Commands.deleteAsync(command);
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde gelöscht!"));
                    }
                    else
                    {
                        SLogger.Log(LogLevel.Debug, "Modifying command", "CCEvents.cs");
                        await server.Commands.modifyAsync(command, value);
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde bearbeitet!"));
                    }
                }
                else
                {
                    if (value != "")
                    {
                        SLogger.Log(LogLevel.Debug, "Creating command", "CCEvents.cs");
                        await server.Commands.createAsync(command, value);
                        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Der Command !{command} wurde erstellt!"));
                    }
                    else await e.Interaction.DeleteOriginalResponseAsync();
                }
            }
            catch (Exception ex)
            {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Ein Fehler ist aufgetreten!"));
                SLogger.Log(LogLevel.Error, $"Couldn't access Custom Command", "CCEvents.cs", ex);
            }

        }
    }
}