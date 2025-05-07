using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace SophBot.Commands.ContextChecks {
    public class RequireBotOwnerCheck : IContextCheck<RequireApplicationOwnerAttribute>
    {
        public async ValueTask<string?> ExecuteCheckAsync(RequireApplicationOwnerAttribute a, CommandContext ctx)
        {
            #pragma warning disable CS8604
            var application = await ctx.Client.GetCurrentApplicationAsync();
            if (ctx.User == application.Owners.First()) return null;
            else return "Only the Developer of this bot can use this command";
        }
    }
}