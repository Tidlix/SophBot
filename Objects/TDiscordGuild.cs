using DSharpPlus.Entities;

namespace SophBot.Objects
{
    public class TDiscordGuild
    {
        private DiscordGuild _guild;

        public enum Channel
        {
            Welcome,
            Rules
        }
        public enum Role
        {
            Member,
            Notification
        }

        public TDiscordGuild(DiscordGuild guild)
        {
            _guild = guild;
        }

        #region GeneralConfig
        public async ValueTask createDefaultConfigAsnyc()
        {
            try
            {
                DiscordChannel? rulesChannel = await _guild.GetRulesChannelAsync();
                DiscordChannel? defaultChannel = _guild.GetDefaultChannel()!;
                DiscordRole defaultRole = _guild.EveryoneRole;

                if (defaultChannel == null!) throw new Exception("Couldn't find default channel."); // Warnings: CS8604 and CS8625
                rulesChannel ??= defaultChannel!;

                await TDatabase.ServerConfig.createAsnyc(_guild.Id, rulesChannel.Id, defaultChannel.Id, defaultRole.Id, defaultRole.Id);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't delete server config > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Couldn't create server config! Please contact the developer of this bot!");
            }
        }
        public async ValueTask deleteConfigAsnyc()
        {
            try
            {
                await TDatabase.ServerConfig.deleteAsync(_guild.Id);
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't delete server config > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Couldn't delete server config!");
            }
            
        }
        #endregion

        #region CustomCommands
        public async ValueTask createCommandAsnyc(string command, string output)
        {
            await TDatabase.CustomCommands.createAsnyc(command, output, _guild.Id);
        }
        public async ValueTask modifyCommandAsnyc(string command, string output)
        {
            await TDatabase.CustomCommands.modifyAsync(command, output, _guild.Id);
        }
        public async ValueTask deleteCommandAsync(string command)
        {
            await TDatabase.CustomCommands.deleteAsnyc(command, _guild.Id);
        }
        public async ValueTask<string?> getCommandAsnyc(string command)
        {
            try
            {
                return await TDatabase.CustomCommands.getCommandAsnyc(command, _guild.Id);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Channels
        public async ValueTask<DiscordChannel> getChannelAsnyc(Channel channel)
        {
            try
            {
                if (channel == Channel.Welcome) return await _guild.GetChannelAsync(await TDatabase.ServerConfig.readValueAsync("welcomechannel", _guild.Id));
                else if (channel == Channel.Rules) return await _guild.GetChannelAsync(await TDatabase.ServerConfig.readValueAsync("rulechannel", _guild.Id));
                else throw new Exception("Unknown Channel type");
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get guild channel > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Database Error!");
            }

        }
        public async ValueTask setChannelAsnyc(Channel channel, DiscordChannel discordChannel)
        {
            try
            {
                if (channel == Channel.Welcome) await TDatabase.ServerConfig.modifyValueAsnyc("welcomechannel", discordChannel.Id.ToString(), _guild.Id);
                else if (channel == Channel.Rules) await TDatabase.ServerConfig.modifyValueAsnyc("rulechannel", discordChannel.Id.ToString(), _guild.Id);
                else throw new Exception("Unknown Channel type");
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get guild channel > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Database Error!");
            }
        }
        #endregion

        #region Roles
        public async ValueTask<DiscordRole> getRoleAsync(Role role)
        {
            try
            {
                if (role == Role.Member) return await _guild.GetRoleAsync(await TDatabase.ServerConfig.readValueAsync("memberrole", _guild.Id));
                else if (role == Role.Notification) return await _guild.GetRoleAsync(await TDatabase.ServerConfig.readValueAsync("mentionrole", _guild.Id));
                else throw new Exception("Unknown Channel type");
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get guild channel > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Database Error!");
            }
        }
        public async ValueTask setRoleAsync(Role role, DiscordRole discordRole)
        {
            try
            {
                if (role == Role.Member) await TDatabase.ServerConfig.modifyValueAsnyc("memberrole", discordRole.Id.ToString(), _guild.Id);
                else if (role == Role.Notification) await TDatabase.ServerConfig.modifyValueAsnyc("mentionrole", discordRole.Id.ToString(), _guild.Id);
                else throw new Exception("Unknown Channel type");
            }
            catch (Exception e)
            {
                TLog.sendLog($"Couldn't get guild channel > {e.Message} <", TLog.MessageType.Error);
                throw new Exception("Database Error!");
            }
        }
        #endregion
    }
}