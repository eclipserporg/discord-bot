using app.Apis;
using app.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
namespace app.Commands;

public class GeneralCommands : BaseCommandModule
{
    private readonly IServerDiscordApi _serverDiscordApi;
    private readonly DiscordService _discordService;

    public GeneralCommands(IServerDiscordApi serverDiscordApi, DiscordService discordService)
    {
        _serverDiscordApi = serverDiscordApi;
        _discordService = discordService;
    }

    [Command("verify")]
    public async Task HelpVerifyCommand(CommandContext ctx, DiscordMember targetMember)
    {
        if (ctx.Channel != DiscordService.Instance.HelpVerifyChannel)
            return;

        await ctx.RespondAsync(
            string.Format("Hello, <@{0}>! Check out the <#{1}> channel for detailed steps on how to join our roleplay server and on how to obtain access to our member-only channels.",
            ctx.User.Id, DiscordService.Instance.VerificationChannel.Id));
    }

    [Command("remove-read-only")]
    public async Task RemoveReadOnlyCommand(CommandContext ctx, DiscordMember targetMember)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        var senderMember = ctx.Member;

        if (targetMember == null || senderMember == null)
        {
            await ctx.RespondAsync("```SUCH MEMBERS DO NOT EXIST```");
            return;
        }

        if (targetMember.Id == senderMember.Id)
        {
            await ctx.RespondAsync("```YOU CAN'T DO THIS ACTION ON YOURSELF```");
            return;
        }

        if (targetMember.Roles.All(x => x.Id != _discordService.ReadOnlyRole.Id))
        {
            await ctx.RespondAsync("```TARGET DOESN'T HAVE READ-ONLY ROLE```");
            return;
        }

        var result = await _serverDiscordApi.PostRemoveReadOnly(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username);

        if(result)
        {
            await targetMember.RevokeRoleAsync(_discordService.ReadOnlyRole, "Removed read-only");
        }
    }

    [Command("commands")]
    public async Task ListCommand(CommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        var message = "```\n";
        message += "!ban;user_id;reason\n";
        message += "!kick;user_id;reason\n";
        message += "!read-only;user_id;reason\n";
        message += "!remove-read-only;user_id\n";
        message += "!save\n";
        message += "!apps\n";
        message += "!ann;text\n";
        message += "!restart\n";
        message += "!quizstats\n";
        message += "```";

        await ctx.RespondAsync(message);
    }

    [Command("read-only")]
    public async Task ReadOnlyCommand(CommandContext ctx, DiscordMember targetMember, string reason)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        var senderMember = ctx.Member;

        if (targetMember == null || senderMember == null)
        {
            await ctx.RespondAsync("```SUCH MEMBERS DO NOT EXIST```");
            return;
        }

        if (targetMember.Id == senderMember.Id)
        {
            await ctx.RespondAsync("```YOU CAN'T DO THIS ACTION ON YOURSELF```");
            return;
        }

        if (targetMember.Roles.Any(x => x.Id == _discordService.ReadOnlyRole.Id))
        {
            await ctx.RespondAsync("```TARGET ALREADY HAS READ-ONLY ROLE```");
            return;
        }

        if(await _serverDiscordApi.PostReadOnly(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await targetMember.GrantRoleAsync(_discordService.ReadOnlyRole);
        }
    }

    [Command("apps")]
    public async Task AppsCommand(CommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        var waitingIssuers = await _serverDiscordApi.GetApps();
        await ctx.RespondAsync(waitingIssuers);
    }

    [Command("ann")]
    public async Task AnnCommand(CommandContext ctx, string text)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        await _serverDiscordApi.PostAnnouncement(text);
    }

    [Command("save")]
    public async Task SaveCommand(CommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        if(await _serverDiscordApi.PostSave())
        {
            await ctx.RespondAsync("Saved to database!");
        }
        else
        {
            await ctx.RespondAsync("Failed to save to database!");
        }
    }

    [Command("kick")]
    public async Task KickCommand(CommandContext ctx, DiscordMember targetMember, string reason)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        var senderMember = ctx.Member;

        if (targetMember == null || senderMember == null)
        {
            await ctx.RespondAsync("```SUCH MEMBERS DO NOT EXIST```");
            return;
        }

        if (targetMember.Id == senderMember.Id)
        {
            await ctx.RespondAsync("```YOU CAN'T DO THIS ACTION ON YOURSELF```");
            return;
        }

        if(await _serverDiscordApi.PostKick(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await targetMember.RemoveAsync(reason);
        }
    }

    [Command("ban")]
    public async Task BanCommand(CommandContext ctx, DiscordMember targetMember, string reason)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        var senderMember = ctx.Member;

        if (targetMember == null || senderMember == null)
        {
            await ctx.RespondAsync("```SUCH MEMBERS DO NOT EXIST```");
            return;
        }

        if (targetMember.Id == senderMember.Id)
        {
            await ctx.RespondAsync("```YOU CAN'T DO THIS ACTION ON YOURSELF```");
            return;
        }

        if(await _serverDiscordApi.PostBan(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await targetMember.BanAsync(0, reason);
        }
    }
}
