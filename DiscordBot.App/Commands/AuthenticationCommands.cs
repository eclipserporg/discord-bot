using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace DiscordBot.Commands;

public class AuthenticationCommands(DiscordService discordService, IServerDiscordApi serverDiscordApi)
{
    [Command("login")]
    [Description("Link your Discord user to your in-game account.")]
    [AllowDMUsage]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("account")] [Description("your in-game login name")] string username, [Parameter("password")] [Description("your in-game login password")] string password)
    {
        if (ctx.Channel.Type != DiscordChannelType.Private)
        {
            await ctx.Member!.SendMessageAsync("Please do not use the **/login** command in public channels as doing so could compromise your in-game account! It is also recommended to use the **\"Dismiss message\"** feature to permanently hide your credentials.");
            return;
        }

        var discordMember = await discordService.Guild.GetMemberAsync(ctx.User.Id);

        if (discordMember != null)
        {
            var response = await serverDiscordApi.PostLogin(username, discordMember.Id, discordMember.Username, discordMember.Discriminator, discordMember.AvatarUrl, password);

            if (response.Status)
                await discordMember.GrantRoleAsync(discordService.MemberRole);

            await ctx.RespondAsync(response.Message, true);
        }
    }
}
