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
        public async ValueTask AddWarningAsnyc(string reason)
        {
            List<SDBValue> values = new();
            values.Add(new(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            values.Add(new(SDBColumn.UserID, Member.Id.ToString()));
            values.Add(new(SDBColumn.Description, reason));
            values.Add(new(SDBColumn.DateTime, DateTime.Now.AddHours(2).ToString("dd.MM.yyyy - HH:mm")));

            await SDBEngine.InsertAsync(values, SDBTable.Warnings, false);
        }
        public async ValueTask<Dictionary<string, string>> GetWarningsAsync()
        {
            Dictionary<string, string> result = new();
            List<SDBValue> conditions = new();
            conditions.Add(new(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new(SDBColumn.UserID, Member.Id.ToString()));

            List<string>? reasons = await SDBEngine.SelectAsync(SDBTable.Warnings, SDBColumn.Description, conditions, orderBy: SDBColumn.DateTime);
            List<string>? time = await SDBEngine.SelectAsync(SDBTable.Warnings, SDBColumn.DateTime, conditions, orderBy: SDBColumn.DateTime);

            if (reasons == null) result.Add("Noch keine Warnung vorhanden", "");
            else
            {
                for (int i = 0; i < reasons.Count; i++)
                {
                    result.Add(reasons[i], time![i]);
                }
            }

            return result;
        }
        #endregion

        #region Points/UserProfiles
        public async ValueTask CreateProfileAsnyc()
        {
            List<SDBValue> values = new();
            values.Add(new(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            values.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));
            if ((await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.Points, values)) != null)
            {
                SLogger.Log(LogLevel.Warning, "Didn't create user profile - profile allready exists", "SDiscordUser.cs");
                return; // Return if exists
            }


            values.Add(new(SDBColumn.Points, "200"));
            values.Add(new(SDBColumn.Number, "0"));


            await SDBEngine.InsertAsync(values, SDBTable.UserProfiles, true);
        }

        public async ValueTask AddPointsAsnyc(ulong points)
        {
            ulong current = await GetPointsAsnyc();

            List<SDBValue> values = new();
            values.Add(new SDBValue(SDBColumn.Points, (current + points).ToString()));
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));

            await SDBEngine.ModifyAsync(values, SDBTable.UserProfiles, conditions);
        }
        public async ValueTask RemovePointsAsnyc(ulong points)
        {
            ulong current = await GetPointsAsnyc();

            List<SDBValue> values = new();
            values.Add(new SDBValue(SDBColumn.Points, (current - points).ToString()));
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));

            await SDBEngine.ModifyAsync(values, SDBTable.UserProfiles, conditions);
        }
        public async ValueTask SetPointsAsnyc(ulong points)
        {
            List<SDBValue> values = new();
            values.Add(new SDBValue(SDBColumn.Points, points.ToString()));
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));

            await SDBEngine.ModifyAsync(values, SDBTable.UserProfiles, conditions);
        }
        public async ValueTask<ulong> GetPointsAsnyc()
        {
            ulong points;
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));

            ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.Points, conditions))!.First(), out points);

            return points;
        }

        public async ValueTask AddMessageCountAsnyc()
        {
            ulong current = await GetMessageCountAsnyc();

            List<SDBValue> values = new();
            values.Add(new SDBValue(SDBColumn.Number, (current + 1).ToString()));
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));

            await SDBEngine.ModifyAsync(values, SDBTable.UserProfiles, conditions);
        }
        public async ValueTask<ulong> GetMessageCountAsnyc()
        {
            ulong messages;
            List<SDBValue> conditions = new();
            conditions.Add(new SDBValue(SDBColumn.ServerID, Member.Guild.Id.ToString()));
            conditions.Add(new SDBValue(SDBColumn.UserID, Member.Id.ToString()));

            ulong.TryParse((await SDBEngine.SelectAsync(SDBTable.UserProfiles, SDBColumn.Number, conditions))!.First(), out messages);

            return messages;
        }
        #endregion
    }
}