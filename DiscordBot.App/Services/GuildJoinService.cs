using DiscordBot.Apis;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;

namespace DiscordBot.Services;

public class GuildJoinService
{
    private readonly DiscordService _discordService;
    private readonly IServerDiscordApi _serverDiscordApi;

    public GuildJoinService(DiscordService discordService, IServerDiscordApi serverDiscordApi)
    {
        _discordService = discordService;
        _serverDiscordApi = serverDiscordApi;

        discordService.SetGuildMemberAddedHandler(OnGuildMemberAdd);
        Log.Information("GuildJoinService: handler registered");
    }

    private async Task OnGuildMemberAdd(DiscordClient client, GuildMemberAddedEventArgs e)
    {
        Log.Information("GuildJoinService: member added {UserId} to guild {GuildId}", e.Member.Id, e.Guild.Id);

        if (e.Guild != _discordService.Guild)
        {
            Log.Information("GuildJoinService: guild mismatch, ignoring");
            return;
        }

        await HandleGuildJoin(e);
    }

    private async Task HandleGuildJoin(GuildMemberAddedEventArgs e)
    {
        var status = await _serverDiscordApi.GetAccountStatus(e.Member.Id);
        Log.Information("GuildJoinService: account status for {UserId} is '{Status}'", e.Member.Id, status);

        switch (status)
        {
            case "Banned":
                await e.Member.GrantRoleAsync(_discordService.BannedRole);
                Log.Information("GuildJoinService: granted BannedRole to {UserId}", e.Member.Id);
                return;
            case "Member":
                await e.Member.GrantRoleAsync(_discordService.MemberRole);
                Log.Information("GuildJoinService: granted MemberRole to {UserId}", e.Member.Id);
                return;
            default:
                Log.Information("GuildJoinService: no role granted, unrecognized status '{Status}'", status);
                return;
        }
    }
}
