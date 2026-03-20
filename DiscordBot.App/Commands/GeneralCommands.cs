using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Serilog;

namespace DiscordBot.Commands;

public class GeneralCommands
{
    private readonly IServerDiscordApi _serverDiscordApi;
    private readonly DiscordService _discordService;

    public GeneralCommands(IServerDiscordApi serverDiscordApi, DiscordService discordService)
    {
        _serverDiscordApi = serverDiscordApi;
        _discordService = discordService;
    }

    [Command("verify")]
    [Description("help user with verification.")]
    public async Task HelpVerifyCommand(SlashCommandContext ctx)
    {
        if (ctx.Channel != _discordService.HelpVerifyChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }

        Log.Information("HelpVerifyCommand");
        var message = $"Hello, {ctx.User.Mention}! Check out the {_discordService.VerificationChannel.Mention} channel for detailed steps on how to join our roleplay server and on how to obtain access to our member-only channels.";
        await ctx.RespondAsync(message);
    }

    [Command("pingserver")]
    [Description("ping server")]
    public async Task PingServerCommand(SlashCommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }

        Log.Information("PingServerCommand");

        var response = await _serverDiscordApi.GetPing();

        if (response)
        {
            Log.Information("PingServerCommand: Success");
            await ctx.RespondAsync("Server is online!");
        }
    }

    [Command("remove-read-only")]
    [Description("remove read only from user")]
    public async Task RemoveReadOnlyCommand(SlashCommandContext ctx, [Parameter("target")] [Description("target member to remove read only")] DiscordUser targetMember)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("RemoveReadOnlyCommand");

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

        var member = await _discordService.Guild.GetMemberAsync(targetMember.Id);

        if (member.Roles.All(x => x.Id != _discordService.ReadOnlyRole.Id))
        {
            await ctx.RespondAsync("```TARGET DOESN'T HAVE READ-ONLY ROLE```");
            return;
        }

        var result = await _serverDiscordApi.PostRemoveReadOnly(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username);

        if (result)
        {
            await member.RevokeRoleAsync(_discordService.ReadOnlyRole, "Removed read-only");
            await ctx.RespondAsync("Removed read-only role");
            return;
        }

        await ctx.RespondAsync("Failed to remove read-only role");
    }

    [Command("read-only")]
    [Description("put read only on user")]
    public async Task ReadOnlyCommand(SlashCommandContext ctx, [Parameter("target")] [Description("target member")] DiscordUser targetMember, [Parameter("reason")] [Description("reason for this action")] string reason)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;
        Log.Information("ReadOnlyCommand");

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

        var member = await _discordService.Guild.GetMemberAsync(targetMember.Id);
        if (member.Roles.Any(x => x.Id == _discordService.ReadOnlyRole.Id))
        {
            await ctx.RespondAsync("```TARGET ALREADY HAS READ-ONLY ROLE```");
            return;
        }

        if (await _serverDiscordApi.PostReadOnly(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await member.GrantRoleAsync(_discordService.ReadOnlyRole);
            await ctx.RespondAsync("Added read-only role");
            return;
        }

        await ctx.RespondAsync("Failed to add read-only role");
    }

    [Command("apps")]
    [Description("get active applications")]
    public async Task AppsCommand(SlashCommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("AppsCommand");

        var waitingIssuers = await _serverDiscordApi.GetApps();
        await ctx.RespondAsync(waitingIssuers);
    }

    [Command("ann")]
    [Description("post announcement in server")]
    public async Task AnnCommand(SlashCommandContext ctx, [Parameter("announcement")] [Description("announcement to post on the server")] string text)
    {
        Log.Information("AnnCommand");
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }

        await _serverDiscordApi.PostAnnouncement(text);
        await ctx.RespondAsync("Announcement sent!");
    }

    [Command("save")]
    [Description("save server state")]
    public async Task SaveCommand(SlashCommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("SaveCommand");

        if (await _serverDiscordApi.PostSave())
        {
            await ctx.RespondAsync("Saved to database!");
        }
        else
        {
            await ctx.RespondAsync("Failed to save to database!");
        }
    }

    [Command("kick")]
    [Description("kick user")]
    public async Task KickCommand(SlashCommandContext ctx, [Parameter("target")] [Description("target member")] DiscordUser targetMember, [Parameter("reason")] [Description("reason for this action")] string reason)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("KickCommand");

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

        if (await _serverDiscordApi.PostKick(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            var member = await _discordService.Guild.GetMemberAsync(targetMember.Id);
            await member.RemoveAsync(reason);
            await ctx.RespondAsync("Kicked user");
            return;
        }

        await ctx.RespondAsync("Failed to kick user");
    }

    [Command("ban")]
    [Description("ban user")]
    public async Task BanCommand(SlashCommandContext ctx, [Parameter("target")] [Description("target member")] DiscordUser targetMember, [Parameter("reason")] [Description("reason for this action")] string reason)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("BanCommand");

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

        if (await _serverDiscordApi.PostBan(nameof(_discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await _discordService.Guild.BanMemberAsync(targetMember.Id, TimeSpan.Zero, reason);
            await ctx.RespondAsync("Banned user");
            return;
        }

        await ctx.RespondAsync("Failed to ban user");
    }
}
