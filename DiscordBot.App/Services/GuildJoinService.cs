using DiscordBot.Apis;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace DiscordBot.Services;

public class GuildJoinService
{
    private readonly DiscordService _discordService;
    private readonly IServerDiscordApi _serverDiscordApi;

    public GuildJoinService(DiscordService discordService, IServerDiscordApi serverDiscordApi)
    {
        _discordService = discordService;
        _serverDiscordApi = serverDiscordApi;

        discordService.Client.GuildMemberAdded += OnGuildMemberAdd;
    }

    private async Task OnGuildMemberAdd(DiscordClient client, GuildMemberAddEventArgs e)
    {
        if (e.Guild != _discordService.Guild)
            return;

        await HandleGuildJoin(e);
    }

    private async Task HandleGuildJoin(GuildMemberAddEventArgs e)
    {
        var status = await _serverDiscordApi.GetAccountStatus(e.Member.Id);

        switch (status)
        {
            case "Banned":
                await e.Member.GrantRoleAsync(_discordService.BannedRole);
                return;
            case "Member":
                await e.Member.GrantRoleAsync(_discordService.MemberRole);
                return;
            case "Donator":
                await e.Member.GrantRoleAsync(_discordService.MemberRole);
                await e.Member.GrantRoleAsync(_discordService.DonatorRole);
                return;
        }
    }
}
