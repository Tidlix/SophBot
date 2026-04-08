using System.ComponentModel;
using System.Data;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SophBot.Universal;

namespace SophBot.Discord.SlashCommands
{
    [Command("Profile")]
    public class ProfileCommands
    {
        [Command("Show"), DefaultGroupCommand]
        public async Task ShowProfile(CommandContext ctx, [Description("Möchtest du ein anderes Profil als dein eigenes anzeigen?")] DiscordMember? target = null)
        {
            await ctx.DeferResponseAsync();
            target ??= ctx.Member;
            Profile profile = new Profile(target!.Id);

            List<DiscordComponent> components = new();
            components.Add(new DiscordTextDisplayComponent($"# {target.Mention}`s Profil"));
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordSectionComponent(
                new DiscordTextDisplayComponent($"## Discord: \n**Name:** {target.DisplayName}\n**Gesendete Nachrichten:** {profile.DiscordMessages}\n**Mitglied seit:** {target.JoinedAt.ToString("dd.MM.yyyy (HH:mm)")}"),
                new DiscordThumbnailComponent(target.AvatarUrl, "Profilbild auf Discord")) 
            );
            components.Add(new DiscordSeparatorComponent(true));
            /*if (profile.TwitchUser is null)
                components.Add(new DiscordTextDisplayComponent($"## Twitch: \nNicht Synchronisiert! (/profile sync)"));
            else
                components.Add(new DiscordSectionComponent(
                    new DiscordTextDisplayComponent($"## Twitch: \n**Name:** {profile.TwitchUser.DisplayName}\n**Gesendete Nachrichten:** {profile.TwitchMessages}"),
                    new DiscordThumbnailComponent(profile.TwitchUser.ProfileImageUrl, "Profilbild auf Twitch")) 
                );*/
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent($"## Allgemeine Statistiken: \n**Insgesamt gesendete Nachrichten:** {profile.DiscordMessages+profile.TwitchMessages}\n**Channelpoints:** {profile.Channelpoints}"));
            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, target.Color.PrimaryColor)));
        }

        /*[Command("Sync")]
        public async Task SyncProfile(SlashCommandContext ctx, [Description("Schreibe hier den Namen des Twitch-Accounts, welchen du mit diesem Profil verbinden willst")] string TwitchName)
        {
            Profile discord = new Profile(ctx.User.Id);
            if (discord.TwitchUser is not null)
            {
                await ctx.RespondAsync("Du hast bereits einen Twitch Account verbunden! \n-# Ist das ein Fehler? Bitte kontaktiere Tidlix!");
            }
            TwitchUser twitchUser = await TwitchEngine.TwitchSharpClient.GetUserByLoginAsync(TwitchName.ToLower());
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

            var mdlResponse = await DiscordEngine.Interactivity.WaitForModalAsync($"SyncTwitchUserMdl.{ctx.User.Id}", TimeSpan.FromMinutes(5));

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
                {
                    twitch.SyncDiscordAccount(discord);
                    DatabaseEngine.DeleteData(DatabaseEngine.DBTable.Profiles, [new DatabaseEngine.DBCondition("id", "=", discord.ID)]);
                }
                else
                {
                    discord.SyncTwitchAccount(twitch);
                    DatabaseEngine.DeleteData(DatabaseEngine.DBTable.Profiles, [new DatabaseEngine.DBCondition("id", "=", twitch.ID)]);
                }
                await mdlResponse.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Synchronisation abgeschlossen!"));
            }
            else
            {
                await mdlResponse.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Synchronisation fehlgeschlagen! - Falscher Code"));
            }
        }*/

        [Command("Leaderboard")]
        public async Task Leaderboard(CommandContext ctx, [Description("Welches Leaderboard möchtest du anzeigen?")] LeaderboardType leaderboard)
        {
            await ctx.DeferResponseAsync();
            
            string column;
            string valueType;
            int place = 1;
            string result = string.Empty;
            List<DiscordComponent> components = new();

            switch (leaderboard)
            {
                case LeaderboardType.Channelpoints: 
                    column = "channel-points";
                    valueType = "Channelpoints";
                    break;
                case LeaderboardType.Nachrichten_Discord:
                    column = "discord-messages";
                    valueType = "gesendete Discord Nachrichten";
                    break;
                case LeaderboardType.Nachrichten_Twitch:
                    column = "twitch-messages";
                    valueType = "gesendete Twitch Nachrichten";
                    break;
                default:
                    column = "unknown";
                    valueType = "unknown";
                    break;
            }
            var lb = DatabaseEngine.SelectTopEntrys(DatabaseEngine.DBTable.Profiles, 10, column, true);

            components.Add(new DiscordTextDisplayComponent($"# Leaderboard ({valueType})"));
            
            foreach(DataRow current in lb.Rows)
            {
                Profile profile = new Profile((long)current["id"]);
                string name = string.Empty;
                string url = string.Empty;
                long value = (long)current[column];
                /*if (profile.TwitchUser is null && profile.DiscordUser is null)
                {
                    name = $"Unbekannter Nutzer (ID {(long)current["id"]})";
                } else
                {
                    switch (leaderboard)
                    {
                        case LeaderboardType.Channelpoints: 
                            name = profile.DiscordUser is not null ? profile.DiscordUser.Mention : profile.TwitchUser!.DisplayName;
                            url = profile.DiscordUser is not null ? profile.DiscordUser.AvatarUrl : profile.TwitchUser!.ProfileImageUrl;
                            break;
                        case LeaderboardType.Nachrichten_Discord:
                            name = profile.DiscordUser is not null ? profile.DiscordUser.Mention : profile.TwitchUser!.DisplayName;
                            url = profile.DiscordUser is not null ? profile.DiscordUser.AvatarUrl : profile.TwitchUser!.ProfileImageUrl;
                            break;
                        case LeaderboardType.Nachrichten_Twitch:
                            name = profile.TwitchUser is not null ? profile.TwitchUser.DisplayName : "Gelöschter Account!";
                            url = profile.TwitchUser is not null ? profile.TwitchUser!.ProfileImageUrl : TwitchEngine.TwitchSharpClient.CurrentUser.ProfileImageUrl;
                            break;
                        default:
                            name = "ERROR - UNKNOWN TYPE!";
                            url = TwitchEngine.TwitchSharpClient.CurrentUser.ProfileImageUrl;
                            break;
                    }
                }
                url ??= TwitchEngine.TwitchSharpClient.CurrentUser.ProfileImageUrl;
                */

                if (place <= 3)
                {
                    components.Add(new DiscordSeparatorComponent(true));
                    components.Add(new DiscordSectionComponent(
                        new DiscordTextDisplayComponent($"{(place == 1 ? "##" : "###")} {place}. {name}\n{value} {valueType}"),
                        new DiscordThumbnailComponent(url)
                    ));
                }
                else
                {
                    result += $"**{place}. {name}** {value} {valueType}\n";
                }
                place++;
            }
            components.Add(new DiscordSeparatorComponent(true));
            components.Add(new DiscordTextDisplayComponent(result));

            await ctx.EditResponseAsync(new DiscordMessageBuilder().EnableV2Components().AddContainerComponent(new (components, false, DiscordColor.Gold)));
        }
        public enum LeaderboardType
        {
            Channelpoints,
            Nachrichten_Discord,
            Nachrichten_Twitch,
        }
        
        [Command("Gift"), Description("Schenke einem anderen Profil deine Punkte!")]
        public async Task GiftPoints(CommandContext ctx, DiscordMember member, long points)
        {
            await ctx.DeferResponseAsync();
            Profile sender = new (ctx.Member!.Id);
            Profile receiver = new (member.Id);

            if (sender == receiver)
            {
                await ctx.EditResponseAsync("Du kannst dir nicht selber Punkte schenken!");
                return;
            }
            if (points <= 0)
            {
                await ctx.EditResponseAsync("Du musst mehr als 0 Punkte verschenken!");
                return;
            }
            if (sender.Channelpoints < points)
            {
                await ctx.EditResponseAsync("Du kannst nicht mehr Punkte verschenken als du hast!");
                return;
            }

            sender.RemoveChannelpoints(points);
            receiver.AddChannelpoints(points);

            await ctx.EditResponseAsync($"Du hast {points} Punkte an {member.Mention} gesendet!");
        }
    }
}