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

    [HttpGet(Name = "isNitroBooster")]
    public async Task<bool> GetIsNitroBooster(ulong id)
    {
        Log.Information("Get isNitroBooster");

        try
        {
            var discordMember = await _discordService.Guild.GetMemberAsync(id);
            return discordMember.PremiumSince.HasValue;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    [HttpGet(Name = "hasPrimaryGuild")]
    public async Task<bool> GetHasPrimaryGuild(ulong id)
    {
        Log.Information("Get hasPrimaryGuild");

        try
        {
            var discordUser = await _discordService.Client.GetUserAsync(id);
            return discordUser.PrimaryGuild?.IdentityGuildId == _discordService.Guild.Id;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    [HttpPost(Name = "grantRole")]
    public async Task PostGrantRole(ulong id, string role)
    {
        Log.Information("Post grantRole");

        try
        {
            var member = await _discordService.Guild.GetMemberAsync(id);

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
        catch (NotFoundException) { }
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
        await foreach (var member in _discordService.Guild.GetAllMembersAsync())
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
                if (_discordService.Guild.Roles.TryGetValue(roleVIP, out var guildRoleVIP))
                    await member.GrantRoleAsync(guildRoleVIP);
            }

            if (isDonator && !member.Roles.Any((role) => role.Id == roleDonator))
            {
                if (_discordService.Guild.Roles.TryGetValue(roleDonator, out var guildRoleDonator))
                    await member.GrantRoleAsync(guildRoleDonator);
            }

            if (isContentCreator && !member.Roles.Any((role) => role.Id == _discordService.CreatorRole.Id))
            {
                await member.GrantRoleAsync(_discordService.CreatorRole);
            }
        }
    }
}