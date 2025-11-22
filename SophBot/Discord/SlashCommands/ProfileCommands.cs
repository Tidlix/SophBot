using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using SophBot.Twitch;
using SophBot.Universal;
using TwitchSharp.Entities;

namespace SophBot.Discord.SlashCommands
{
    [Command("Profile")]
    public class ProfileCommands
    {
        [Command("Sync")]
        public async Task SyncProfile(SlashCommandContext ctx, [Description("Schreibe hier den Namen des Twitch-Accounts, welchen du mit diesem Profil verbinden willst")] string TwitchName)
        {
            Profile discord = new Profile(ctx.User.Id);
            if (discord.TwitchUser is not null)
            {
                await ctx.RespondAsync("Du hast bereits einen Twitch Account verbunden! \n-# Ist das ein Fehler? Bitte kontaktiere Tidlix!");
            }
            TwitchUser twitchUser = await TwitchEngine.Client.GetUserByLoginAsync(TwitchName.ToLower());
            Profile twitch = new Profile(twitchUser.ID);
            if (twitch.DiscordUser is not null)
            {
                await ctx.RespondAsync("Dieser Twitch Account wurde bereits mit einem anderen Discord Account verbunden! \n-# Ist das ein Fehler? Bitte kontaktiere Tidlix!");
            }

            DiscordModalBuilder modal = new DiscordModalBuilder()
                .WithCustomId($"SyncTwitchUserMdl.{ctx.User.Id}")
                .WithTitle("Twitch-Account Verbinden...")
                .AddTextDisplay($"Der Synchronisationscode wurde via. DM an den Twitch Account `{twitchUser.DisplayName}` gesendet.")
                .AddTextInput(new DiscordTextInputComponent($"SyncTwitchUserInp.{ctx.User.Id}", placeholder: "000000", min_length: 6, max_length: 6), "Synchronisationscode:", "Gib hier deinen Synchronisationscode ein!")
                .AddTextDisplay("-# Du hast nur 5 Minuten Zeit den Code einzugeben!")
                .AddTextDisplay("-# Hast du keine DM erhalten? Überprüfe ob du deinen Twitch Namen richtig geschrieben hast und versuche es erneut!");
            await ctx.RespondWithModalAsync(modal);

            int rand = new Random().Next(100000, 999999);
            await twitchUser.SendWhisperAsync($"Dein Synchronisationscode lautet '{rand}'. Gib diesen Code niemanden weiter. Du hast nicht versucht dich anzumelden? In dem Fall ignorier diese Nachricht einfach!");

            InteractivityExtension interactivity = (ctx.Client.ServiceProvider.GetService(typeof(InteractivityExtension)) as InteractivityExtension)!;
            var mdlResponse = await interactivity.WaitForModalAsync($"SyncTwitchUserMdl.{ctx.User.Id}", TimeSpan.FromMinutes(5));

            await mdlResponse.Result.Interaction.DeferAsync();
            if (mdlResponse.TimedOut)
            {
                await mdlResponse.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Die Antwort hat zu lange gedauert!"));
                return;
            }

            var userInput = mdlResponse.Result.Values[$"SyncTwitchUserInp.{ctx.User.Id}"] as TextInputModalSubmission;

            if (userInput!.Value == rand.ToString())
            {
                if (discord.ID > twitch.ID)
                    twitch.SyncDiscordAccount(discord);
                else
                    discord.SyncTwitchAccount(twitch);
                await mdlResponse.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Synchronisation abgeschlossen!"));
            }
            else
            {
                await mdlResponse.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Synchronisation fehlgeschlagen! - Falscher Code"));
            }
        }
    }
}