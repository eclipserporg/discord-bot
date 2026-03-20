using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Entities;

namespace DiscordBot.Commands;

public class AuthenticationCommands
{
    private readonly DiscordService _discordService;
    private readonly IServerDiscordApi _serverDiscordApi;

    public AuthenticationCommands(DiscordService discordService, IServerDiscordApi serverDiscordApi)
    {
        _discordService = discordService;
        _serverDiscordApi = serverDiscordApi;
    }

    [Command("login")]
    [Description("Use to link your Discord user to an in-game account. Use via direct message only!")]
    [InteractionAllowedContexts(DiscordInteractionContextType.BotDM)]
    public async Task ExecuteAsync(SlashCommandContext ctx,
        [Parameter("account")] [Description("your in-game account name")] string username,
        [Parameter("password")] [Description("the password for your in-game account")] string password)
    {
        if (ctx.Channel.Type != DiscordChannelType.Private)
        {
            await ctx.Member!.SendMessageAsync("Please do not use the **/login** command in public channels as it could compromise your in-game account! It is also recommended to click **\"Dismiss message\"** on all of the notifications that contain the bot command to permanently hide your credentials.");
            return;
        }

        var discordMember = await _discordService.Guild.GetMemberAsync(ctx.User.Id);

        if (discordMember != null)
        {
            var response = await _serverDiscordApi.PostLogin(username, discordMember.Id, discordMember.Username, discordMember.Discriminator, discordMember.AvatarUrl, password);

            if (response.Status)
                await discordMember.GrantRoleAsync(_discordService.MemberRole);

            await ctx.RespondAsync(response.Message, true);
        }
    }
}
