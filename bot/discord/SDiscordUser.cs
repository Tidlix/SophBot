using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SophBot.bot.database;
using SophBot.bot.logs;

namespace SophBot.bot.discord
{
    public class SDiscordUser
    {
        private DiscordMember Member;
        public SDiscordUser(DiscordMember member)
        {
            this.Member = member;
        }
        public SDiscordUser(DiscordUser user, DiscordGuild guild)
        {
            this.Member = guild.GetMemberAsync(user.Id).Result;
        }

        #region Warnings
        public async ValueTask AddWarningAsync(string reason)
        {
            Dictionary<string, object>[] values = [
                new Dictionary<string, object> {
                    {"serverid", Member.Guild.Id},
                    {"userid", Member.Id},
                    {"reason", reason},
                    {"datetime", DateTime.Now.AddHours(2).ToString("dd.MM.yyyy - HH:mm")}
                }
            ];

            await SDBEngine.InsertToAsync("warnings", values, false);
        }
        public async ValueTask<Dictionary<string, string>> GetWarningsAsync()
        {
            Dictionary<string, string> result = new();

            var warnings = await SDBEngine.SelectFromAsync("warnings", ["reason", "datetime"], new Dictionary<string, object> { { "serverid", Member.Guild.Id }, { "userid", Member.Id } }, "datetime");

            foreach (var warning in warnings)
            {
                result.Add((string)warning[0], (string)warning[1]);
            }
            ;

            return result;
        }
        #endregion

        #region Points/UserProfiles
        public async ValueTask CreateProfileAsync()
        {
            Dictionary<string, object>[] values = [
                new Dictionary<string, object> {
                    {"server", Member.Guild.Id},
                    {"userid", Member.Id},
                    {"points", 0},
                    {"number", 0}
                }
            ];

            await SDBEngine.InsertToAsync("profiles", values, false);
        }

        public async ValueTask AddPointsAsync(ulong points)
        {
            ulong current = await GetPointsAsync();
            await SDBEngine.ModifyAtAsync("profiles", new Dictionary<string, object> { { "points", current + points } }, new Dictionary<string, object> { { "server", Member.Guild.Id }, { "userid", Member.Id } });
        }
        public async ValueTask RemovePointsAsync(ulong points)
        {
            ulong current = await GetPointsAsync();
            await SDBEngine.ModifyAtAsync("profiles", new Dictionary<string, object> { { "points", current - points } }, new Dictionary<string, object> { { "server", Member.Guild.Id }, { "userid", Member.Id } });
        }
        public async ValueTask SetPointsAsync(ulong points)
        {
            await SDBEngine.ModifyAtAsync("profiles", new Dictionary<string, object> { { "points", points } }, new Dictionary<string, object> { { "server", Member.Guild.Id }, { "userid", Member.Id } });
        }
        public async ValueTask<ulong> GetPointsAsync()
        {
            ulong points = (ulong)(long)(await SDBEngine.SelectFromAsync("profiles", ["points"], new Dictionary<string, object> { { "server", Member.Guild.Id }, { "userid", Member.Id } }))[0][0];
            return points;
        }

        public async ValueTask AddMessageCountAsync()
        {
            ulong current = await GetMessageCountAsnyc();

            await SDBEngine.ModifyAtAsync("profiles", new Dictionary<string, object> { { "number", current + 1 } }, new Dictionary<string, object> { { "server", Member.Guild.Id }, { "userid", Member.Id } });
        }

        public async ValueTask<ulong> GetMessageCountAsnyc()
        {
            ulong messages = (ulong)(long)(await SDBEngine.SelectFromAsync("profiles", ["number"], new Dictionary<string, object> { { "server", Member.Guild.Id }, { "userid", Member.Id } }))[0][0];
            return messages;
        }
        #endregion
    }
}