using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class RREvents :
        IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            string customId = e.Id;     // rrSystem_role={article};

            if (!customId.Contains("rrSystem")) return;

            try
            {
                string pattern = @"role=([^}]+);";
                ulong roleId;


                Match match = Regex.Match(customId, pattern);
                if (match.Success)
                {
                    ulong.TryParse(match.Groups[1].Value, out roleId);
                    SLogger.Log(LogLevel.Debug, $"Got reaction roleId {roleId}", "RREvents.cs");
                }
                else
                {
                    throw new Exception("Coudln't find match in customID!");
                }

                DiscordRole role = await e.Guild.GetRoleAsync(roleId);
                DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);

                if (member.Roles.Contains(role)) await member.RevokeRoleAsync(role);
                else await member.GrantRoleAsync(role);

                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"Rolle `{role.Name}` geändert!"));
            }
            catch (Exception ex)
            {
                await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Ein Fehler ist aufgetreten! "));

                SLogger.Log(LogLevel.Error, "Failed to add/remove role", "RREvents.cs", ex);
            }
        }
    }
}