using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Database;
using SophBot.Messages;

namespace SophBot.EventHandlers {
    class ButtonEvents : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
        {
            switch (e.Id) {
                case "ruleAcceptButton":
                    await acceptRules(e);
                    break;
                default:
                    if (e.Id.Contains("rrButton")) {
                        await reactionRole(e);
                    }
                    break;
            }
        }

        private async Task acceptRules (ComponentInteractionCreatedEventArgs e) {
            #pragma warning disable CS8602

            await e.Interaction.DeferAsync(true);

            DiscordMember member = (DiscordMember) e.Interaction.User;

            try {
                DiscordRole memberRole = await e.Interaction.Guild.GetRoleAsync(await TidlixDB.ServerConfig.readValueAsync("memberrole", e.Interaction.Guild.Id));
                await member.GrantRoleAsync(memberRole);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Regeln erfolgreich akzeptiert!"));
            } catch (Exception ex){
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Fehler - Regeln konnten nicht akzeptiert werden! Bitte kontaktiere den Entwickler dieses Bots!"));
                await Log.sendMessage($"Rule role from {e.Interaction.Guild.Name}({e.Interaction.Guild.Id}) couldn't be readed! - {ex.Message}", MessageType.Error());
            }
        }
        private async Task reactionRole (ComponentInteractionCreatedEventArgs e) {
            await e.Interaction.DeferAsync(true);
            DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);

            string roleIdString = e.Id.Substring(e.Id.IndexOf('_')+1);

            ulong.TryParse(roleIdString, out ulong roleId);
            DiscordRole role = await e.Guild.GetRoleAsync(roleId);

            if (member.Roles.Contains(role)) await member.RevokeRoleAsync(role);
            else await member.GrantRoleAsync(role);
            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Rolle aktualisiert!"));
        }
    }
}