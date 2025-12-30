using DiscordBot.Data.Models;
using DiscordBot.Models;
using DiscordBot.Services;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DiscordBot.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class DiscordController : ControllerBase
{
    private readonly DiscordService _discordService;

    public DiscordController(DiscordService discordService)
    {
        _discordService = discordService;
    }

    [HttpGet(Name = "hasContentCreator")]
    public async Task<bool> GetHasContentCreator(ulong id)
    {
        Log.Information("Get hasContentCreator");
        var member = await _discordService.Guild.GetMemberAsync(id);

        if(member == null)
            return false;

        return member.Roles.Any(x => x.Id == _discordService.CreatorRole.Id);
    }

    [HttpGet(Name = "user")]
    public async Task<DiscordUserDto?> GetUser(ulong id)
    {
        Log.Information("Get user");

        try
        {
            var discordUser = await _discordService.Client.GetUserAsync(id);
            return new()
            {
                Id = discordUser.Id.ToString(),
                Username = discordUser.Username,
                Discriminator = discordUser.Discriminator,
                AvatarUrl = discordUser.AvatarUrl
            };
        }

        catch (NotFoundException)
        {
            return new()
            {
                Id = id.ToString(),
                Username = "unknown discord user",
                Discriminator = string.Empty,
                AvatarUrl = @"https://cdn.discordapp.com/embed/avatars/1.png"
            };
        }
    }

    [HttpPost(Name = "grantRole")]
    public async Task PostGrantRole(ulong id, string role)
    {
        Log.Information("Post grantRole");
        var member = await _discordService.Guild.GetMemberAsync(id);

        if (member == null)
            return;

        var discordRole = role switch
        {
            nameof(_discordService.MemberRole) => _discordService.MemberRole,
            nameof(_discordService.CreatorRole) => _discordService.CreatorRole,
            nameof(_discordService.BannedRole) => _discordService.BannedRole,
            nameof(_discordService.ReadOnlyRole) => _discordService.ReadOnlyRole,
            _ => throw new Exception("Invalid role name")
        };

        await member.GrantRoleAsync(discordRole);
    }

    [HttpPost(Name = "revokeRole")]
    public async Task PostRevokeRole(ulong id, string role)
    {
        Log.Information("Post revokeRole");

        try
        {
            var discordMember = await _discordService.Guild.GetMemberAsync(id);
            var discordRole = role switch
            {
                nameof(_discordService.MemberRole) => _discordService.MemberRole,
                nameof(_discordService.CreatorRole) => _discordService.CreatorRole,
                nameof(_discordService.BannedRole) => _discordService.BannedRole,
                nameof(_discordService.ReadOnlyRole) => _discordService.ReadOnlyRole,
                _ => throw new Exception("Invalid role name")
            };

            await discordMember.RevokeRoleAsync(discordRole);
        }

        catch (NotFoundException) { }
    }

    [HttpPost(Name = "updateMemberRoles")]
    public async Task PostUpdateMemberRoles(DiscordMemberRolesDto memberRoles)
    {
        Log.Information("Post updateMemberRoles");
        var members = await _discordService.Guild.GetAllMembersAsync();

        if (members == null)
            return;

        foreach (var member in members)
        {
            var isVIP = memberRoles.MemberRolesVIP.TryGetValue(member.Id, out var roleVIP);
            var isDonator = memberRoles.MemberRolesDonator.TryGetValue(member.Id, out var roleDonator);
            var isContentCreator = memberRoles.ContentCreatorMemberIds.Contains(member.Id);

            foreach (var role in member.Roles)
            {
                if (memberRoles.RolesVIP.Contains(role.Id) && (!isVIP || role.Id != roleVIP))
                    await member.RevokeRoleAsync(role);

                if (memberRoles.RolesDonator.Contains(role.Id) && (!isDonator || role.Id != roleDonator))
                    await member.RevokeRoleAsync(role);

                if (!isContentCreator && role.Id == _discordService.CreatorRole.Id)
                    await member.RevokeRoleAsync(_discordService.CreatorRole);
            }

            if (isVIP && !member.Roles.Any((role) => role.Id == roleVIP))
            {
                var role = _discordService.Guild.GetRole(roleVIP);
                if (role != null)
                    await member.GrantRoleAsync(role);
            }

            if (isDonator && !member.Roles.Any((role) => role.Id == roleDonator))
            {
                var role = _discordService.Guild.GetRole(roleDonator);
                if (role != null)
                    await member.GrantRoleAsync(role);
            }

            if (isContentCreator && !member.Roles.Any((role) => role.Id == _discordService.CreatorRole.Id))
            {
                await member.GrantRoleAsync(_discordService.CreatorRole);
            }
        }
    }
}