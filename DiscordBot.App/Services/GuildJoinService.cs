using DiscordBot.Apis;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;

namespace DiscordBot.Services;

public class GuildJoinHandler(DiscordService discordService, IServerDiscordApi serverDiscordApi) : IEventHandler<GuildMemberAddedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient sender, GuildMemberAddedEventArgs e)
    {
        if (e.Guild != discordService.Guild)
            return;

        var status = await serverDiscordApi.GetAccountStatus(e.Member.Id);
        Log.Information("GuildJoinHandler: account status for {UserId} is '{Status}'", e.Member.Id, status);

        switch (status)
        {
            case "Banned":
                await e.Member.GrantRoleAsync(discordService.BannedRole);
                Log.Information("GuildJoinHandler: granted BannedRole to {UserId}", e.Member.Id);
                break;
            case "Member":
                await e.Member.GrantRoleAsync(discordService.MemberRole);
                Log.Information("GuildJoinHandler: granted MemberRole to {UserId}", e.Member.Id);
                break;
            default:
                Log.Information("GuildJoinHandler: no role granted, unrecognized status '{Status}'", status);
                break;
        }
    }
}
