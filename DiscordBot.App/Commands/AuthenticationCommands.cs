using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System.Xml.Linq;

namespace DiscordBot.Commands;

public class AuthenticationCommands : ApplicationCommandModule
{
    private readonly DiscordService _discordService;
    private readonly IServerDiscordApi _serverDiscordApi;

    public AuthenticationCommands(DiscordService discordService, IServerDiscordApi serverDiscordApi)
    {
        _discordService = discordService;
        _serverDiscordApi = serverDiscordApi;
    }

    [SlashCommand("login", "Use to link your Discord user to an in-game account. Use via direct message only!", defaultPermission: false)]
    public async Task LoginCommand(InteractionContext ctx, [Option("account", "your in-game account name")] string username, [Option("password", "the password for your in-game account")] string password)
    {
        if (ctx.Channel.Type != ChannelType.Private)
        {
            // Do not provide an interaction context response so the command parameters remain hidden to others. Send a direct message instead.
            await ctx.Member.SendMessageAsync("Please do not use the **/login** command in public channels as it could compromise your in-game account! It is also recommended to click **\"Dismiss message\"** on all of the notifications that contain the bot command to permanently hide your credentials.");
            return;
        }

        var discordMember = await _discordService.Guild.GetMemberAsync(ctx.User.Id);

        if (discordMember != null)
        {
            var response = await _serverDiscordApi.PostLogin(username, discordMember.Id, discordMember.Username, discordMember.Discriminator, discordMember.AvatarUrl, password);

            if (response.Status)
                await discordMember.GrantRoleAsync(_discordService.MemberRole);

            await ctx.CreateResponseAsync(response.Message);
        }
    }
}
