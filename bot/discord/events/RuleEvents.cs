using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SophBot.bot.logs;

namespace SophBot.bot.discord.events
{
    public class RuleEvents :
        IEventHandler<ModalSubmittedEventArgs>,
        IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ModalSubmittedEventArgs e)
        {
            if (!e.Id.Contains("ruleSystem")) return;

            SLogger.Log(LogLevel.Debug, "Got rule modal interaction!", "RuleEvents.cs");

            await e.Interaction.DeferAsync(true);


            var values = e.Values.Values.ToArray();
            string title = values[0];
            string description = values[1];

            SDiscordServer server = new(e.Interaction.Guild!);
            try
            {
                SLogger.Log(LogLevel.Debug, "Try to get rule channel", "RuleEvents.cs");
                var channel = await server.getChannelAsync(SDiscordChannel.RuleChannel);
                SLogger.Log(LogLevel.Debug, $"Got rule channel {channel.Name} ({channel.Id})!", "RuleEvents.cs");


                DiscordComponent[] components = {
                new DiscordTextDisplayComponent($"### {title}"),
                new DiscordSeparatorComponent(),
                new DiscordTextDisplayComponent($"{description}")
                };
                SLogger.Log(LogLevel.Debug, "sending rule message!", "RuleEvents.cs");
                var msg = await channel.SendMessageAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.SpringGreen)));
                SLogger.Log(LogLevel.Debug, "sent rule message", "RuleEvents.cs");
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Regel gesendet! {msg.JumpLink}"));

            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Couldn't send rule message", "RuleEvents.cs", ex);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Ein Fehler ist aufgetreten!"));
            }
        }

        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            if (!e.Id.Contains("ruleSystem")) return;

            SLogger.Log(LogLevel.Debug, "Got rule button interaction!", "RuleEvents.cs");

            await e.Interaction.DeferAsync(true);

            DiscordMember member = (DiscordMember)e.Interaction.User;
            SDiscordServer server = new(e.Guild);

            try
            {
                SLogger.Log(LogLevel.Debug, "Try to get member role!", "RuleEvents.cs");
                DiscordRole memberRole = await server.getRoleAsync(SDiscordRole.MemberRole);
                SLogger.Log(LogLevel.Debug, $"Got member role {memberRole.Name} ({memberRole.Id!})!", "RuleEvents.cs");
                await member.GrantRoleAsync(memberRole);
                SLogger.Log(LogLevel.Debug, $"Granted member role to {member.DisplayName} ({member.Id})!", "RuleEvents.cs");
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Regeln erfolgreich akzeptiert!"));
            }
            catch (Exception ex)
            {
                SLogger.Log(LogLevel.Error, "Couldn't add/remove role", "RuleEvents.cs", ex);
            }
        }
    }
}