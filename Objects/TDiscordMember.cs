using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace SophBot.Objects
{
    public class TDiscordMember
    {
        private DiscordMember _member;

        public TDiscordMember(DiscordMember member)
        {
            _member = member;
        }

        public async ValueTask createProfileAsync()
        {
            try
            {
                await TDatabase.UserProfiles.createAsync(_member.Guild.Id, _member.Id, 100);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get member Warnings > {e.Message} <");
                throw new Exception("Couldn't add user warning. Please contact the developer for more information!");
            }
        }

        #region PointSystem
        public async ValueTask setGuildPointsAsnyc(ulong points)
        {
            try
            {
                await TDatabase.UserProfiles.modifyValueAsnyc("points", points.ToString(), _member.Guild.Id, _member.Id);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't set Points to user > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Couldn't set user points. Please contact the developer for more information!");
            }
        }
        public async ValueTask addGuildPointsAsnyc(ulong points)
        {
            try
            {
                ulong currentPoints = await TDatabase.UserProfiles.getPointsAsync(_member.Guild.Id, _member.Id);
                await TDatabase.UserProfiles.modifyValueAsnyc("points", (currentPoints + points).ToString(), _member.Guild.Id, _member.Id);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't add Points to user > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Couldn't add user points. Please contact the developer for more information!");
            }
        }
        public async ValueTask removeGuildPointsAsnyc(ulong points)
        {
            try
            {
                ulong currentPoints = await TDatabase.UserProfiles.getPointsAsync(_member.Guild.Id, _member.Id);
                if (points > currentPoints) points = currentPoints;
                await TDatabase.UserProfiles.modifyValueAsnyc("points", (currentPoints - points).ToString(), _member.Guild.Id, _member.Id);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't remove Points from user > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Couldn't remove user points. Please contact the developer for more information!");
            }
        }
        public async ValueTask<ulong> getGuildPointsAsnyc()
        {
            try
            {
                ulong currentPoints = await TDatabase.UserProfiles.getPointsAsync(_member.Guild.Id, _member.Id);
                return currentPoints;
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get Points from user > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Couldn't get user points. Please contact the developer for more information!");
            }
        }
        #endregion

        #region WarnSystem 
        public async ValueTask addWarningAsnyc(string reason)
        {
            try
            {
                TUserWarning warning = new(null, _member.Guild.Id, _member.Id, DateTime.Now.ToString("[dd.MM.yyyy - HH:mm]"), reason);
                await TDatabase.Warnings.createAsnyc(warning);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get member Warnings > {e.Message} <");
                throw new Exception("Couldn't add user warning. Please contact the developer for more information!");
            }
        }
        public async ValueTask<List<TUserWarning>> getWarningsAsnyc()
        {
            try
            {
                return await TDatabase.Warnings.getAllByMemberAsnyc(_member);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get member Warnings > {e.Message} <");
                throw new Exception("Couldn't get user warnings. Please contact the developer for more information!");
            }
        }
        public async ValueTask removeWarningAsnyc(ulong warnId)
        {
            try
            {
                await TDatabase.Warnings.deleteAsnyc(warnId);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get member Warnings > {e.Message} <");
                throw new Exception("Couldn't get user warnings. Please contact the developer for more information!");
            }
        }
        #endregion
    }
}