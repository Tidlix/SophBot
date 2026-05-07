using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SophBot.Discord
{
    public class DiscordService : IHostedService
    {
        private readonly ILogger<DiscordService> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly DiscordClient client;
        private DiscordChannel? logchannel = null;
        private DiscordChannel? ccforum = null;

        public DiscordChannel getLogChannel() => logchannel ?? throw new Exception("Configured Logchannel was not found!");
        public DiscordChannel getCCForum() => ccforum ?? throw new Exception("Configured CC-Forum was not found!");
    

        public DiscordService(ILogger<DiscordService> logger, IHostApplicationLifetime appLifetime, DiscordClient client)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.client = client;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ulong logChannelId = ulong.Parse(Environment.GetEnvironmentVariable("Discord.Log-Channel") ?? throw new Exception("LogChannel ID was not found in appsettings.json"));
            ulong ccForumId = ulong.Parse(Environment.GetEnvironmentVariable("Discord.CC-Forum") ?? throw new Exception("CC-Forum ID was not found in appsettings.json"));
            await client.ConnectAsync();
            logchannel = await client.GetChannelAsync(logChannelId);
            ccforum = await client.GetChannelAsync(ccForumId);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await client.DisconnectAsync();
        }
    }
}