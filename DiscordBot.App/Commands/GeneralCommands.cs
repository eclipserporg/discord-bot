using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Entities;
using Serilog;

namespace DiscordBot.Commands;

public class VerifyCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("verify")]
    [Description("help user with verification.")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx)
    {
        if (ctx.Channel != discordService.HelpVerifyChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("HelpVerifyCommand");
        var message = $"Hello, {ctx.User.Mention}! Check out the {discordService.VerificationChannel.Mention} channel for detailed steps on how to join our roleplay server and on how to obtain access to our member-only channels.";
        await ctx.RespondAsync(message);
    }
}

public class PingServerCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("pingserver")]
    [Description("ping server")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx)
    {
        if (ctx.Channel != discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("PingServerCommand");
        var response = await serverDiscordApi.GetPing();
        if (response)
        {
            Log.Information("PingServerCommand: Success");
            await ctx.RespondAsync("Server is online!");
        }
    }
}

public class RemoveReadOnlyCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("remove-read-only")]
    [Description("remove read only from user")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("target")] [Description("target member to remove read only")] DiscordUser targetMember)
    {
        if (ctx.Channel != discordService.CommandsChannel)
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

        var member = await discordService.Guild.GetMemberAsync(targetMember.Id);
        if (member.Roles.All(x => x.Id != discordService.ReadOnlyRole.Id))
        {
            await ctx.RespondAsync("```TARGET DOESN'T HAVE READ-ONLY ROLE```");
            return;
        }

        var result = await serverDiscordApi.PostRemoveReadOnly(nameof(discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username);
        if (result)
        {
            await member.RevokeRoleAsync(discordService.ReadOnlyRole, "Removed read-only");
            await ctx.RespondAsync("Removed read-only role");
            return;
        }
        await ctx.RespondAsync("Failed to remove read-only role");
    }
}

public class ReadOnlyCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("read-only")]
    [Description("put read only on user")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("target")] [Description("target member")] DiscordUser targetMember, [Parameter("reason")] [Description("reason for this action")] string reason)
    {
        if (ctx.Channel != discordService.CommandsChannel)
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

        var member = await discordService.Guild.GetMemberAsync(targetMember.Id);
        if (member.Roles.Any(x => x.Id == discordService.ReadOnlyRole.Id))
        {
            await ctx.RespondAsync("```TARGET ALREADY HAS READ-ONLY ROLE```");
            return;
        }
        if (await serverDiscordApi.PostReadOnly(nameof(discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await member.GrantRoleAsync(discordService.ReadOnlyRole);
            await ctx.RespondAsync("Added read-only role");
            return;
        }
        await ctx.RespondAsync("Failed to add read-only role");
    }
}

public class AppsCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("apps")]
    [Description("get active applications")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx)
    {
        if (ctx.Channel != discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("AppsCommand");
        var waitingIssuers = await serverDiscordApi.GetApps();
        await ctx.RespondAsync(waitingIssuers);
    }
}

public class AnnCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("ann")]
    [Description("post announcement in server")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("announcement")] [Description("announcement to post on the server")] string text)
    {
        Log.Information("AnnCommand");
        if (ctx.Channel != discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        await serverDiscordApi.PostAnnouncement(text);
        await ctx.RespondAsync("Announcement sent!");
    }
}

public class SaveCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("save")]
    [Description("save server state")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx)
    {
        if (ctx.Channel != discordService.CommandsChannel)
        {
            await ctx.RespondAsync("Invalid channel.");
            return;
        }
        Log.Information("SaveCommand");
        if (await serverDiscordApi.PostSave())
            await ctx.RespondAsync("Saved to database!");
        else
            await ctx.RespondAsync("Failed to save to database!");
    }
}

public class KickCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("kick")]
    [Description("kick user")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("target")] [Description("target member")] DiscordUser targetMember, [Parameter("reason")] [Description("reason for this action")] string reason)
    {
        if (ctx.Channel != discordService.CommandsChannel)
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
        if (await serverDiscordApi.PostKick(nameof(discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            var member = await discordService.Guild.GetMemberAsync(targetMember.Id);
            await member.RemoveAsync(reason);
            await ctx.RespondAsync("Kicked user");
            return;
        }
        await ctx.RespondAsync("Failed to kick user");
    }
}

public class BanCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService)
{
    [Command("ban")]
    [Description("ban user")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("target")] [Description("target member")] DiscordUser targetMember, [Parameter("reason")] [Description("reason for this action")] string reason)
    {
        if (ctx.Channel != discordService.CommandsChannel)
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
        if (await serverDiscordApi.PostBan(nameof(discordService.CommandsChannel), ctx.User.Id, ctx.User.Username, targetMember.Id, targetMember.Username, reason))
        {
            await discordService.Guild.BanMemberAsync(targetMember.Id, TimeSpan.Zero, reason);
            await ctx.RespondAsync("Banned user");
            return;
        }
        await ctx.RespondAsync("Failed to ban user");
    }
}
