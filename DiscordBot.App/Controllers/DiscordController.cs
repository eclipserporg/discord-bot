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
        await foreach (var discordMember in _discordService.Guild.GetAllMembersAsync())
        {
            var isVIP = memberRoles.MemberRoleIdsVIP.TryGetValue(discordMember.Id, out var roleIdVIP);
            var isDonator = memberRoles.MemberRoleIdsDonator.TryGetValue(discordMember.Id, out var roleIdDonator);
            var isContentCreator = memberRoles.ContentCreatorMemberIds.Contains(discordMember.Id);

            foreach (var discordMemberRole in discordMember.Roles)
            {
                if (memberRoles.RoleIdsVIP.Contains(discordMemberRole.Id) && (!isVIP || discordMemberRole.Id != roleIdVIP))
                {
                    await discordMember.RevokeRoleAsync(discordMemberRole);
                }
                    
                if (memberRoles.RoleIdsDonator.Contains(discordMemberRole.Id) && (!isDonator || discordMemberRole.Id != roleIdDonator))
                {
                    await discordMember.RevokeRoleAsync(discordMemberRole);
                }
                    
                if (!isContentCreator && discordMemberRole.Id == _discordService.CreatorRole.Id)
                {
                    await discordMember.RevokeRoleAsync(_discordService.CreatorRole);
                }
            }

            if (isVIP && !discordMember.Roles.Any((discordRole) => discordRole.Id == roleIdVIP) && _discordService.Guild.Roles.TryGetValue(roleIdVIP, out var roleVIP))
            {
                await discordMember.GrantRoleAsync(roleVIP);
            }

            if (isDonator && !discordMember.Roles.Any((discordRole) => discordRole.Id == roleIdDonator) && _discordService.Guild.Roles.TryGetValue(roleIdDonator, out var roleDonator))
            {
                await discordMember.GrantRoleAsync(roleDonator);
            }

            if (isContentCreator && !discordMember.Roles.Any((role) => role.Id == _discordService.CreatorRole.Id))
            {
                await discordMember.GrantRoleAsync(_discordService.CreatorRole);
            }
        }
    }
}