using app.Apis;
using app.Models;
using app.Services;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers
{
    [ApiController]
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
            var member = await _discordService.Guild.GetMemberAsync(id);

            if(member == null)
                return false;

            return member.Roles.Any(x => x.Id == _discordService.CreatorRole.Id);
        }

        [HttpGet(Name = "user")]
        public async Task<DiscordUserDto?> GetUser(ulong id)
        {
            var member = await _discordService.Guild.GetMemberAsync(id);

            if (member == null)
                return null;

            return new DiscordUserDto()
            {
                Id = member.Id.ToString(),
                Username = member.Username,
                Discriminator = member.Discriminator,
                AvatarUrl = member.AvatarUrl
            };
        }

        [HttpPost(Name = "grantRole")]
        public async Task PostGrantRole(ulong id, string role)
        {
            var member = await _discordService.Guild.GetMemberAsync(id);

            if (member == null)
                return;

            var discordRole = role switch
            {
                nameof(_discordService.MemberRole) => _discordService.MemberRole,
                nameof(_discordService.CreatorRole) => _discordService.CreatorRole,
                nameof(_discordService.BannedRole) => _discordService.BannedRole,
                nameof(_discordService.ReadOnlyRole) => _discordService.ReadOnlyRole,
                nameof(_discordService.DonatorRole) => _discordService.DonatorRole,
                _ => throw new Exception("Invalid role name")
            };

            await member.GrantRoleAsync(discordRole);
        }

        [HttpPost(Name = "revokeRole")]
        public async Task PostRevokeRole(ulong id, string role)
        {
            var member = await _discordService.Guild.GetMemberAsync(id);

            if (member == null)
                return;

            var discordRole = role switch
            {
                nameof(_discordService.MemberRole) => _discordService.MemberRole,
                nameof(_discordService.CreatorRole) => _discordService.CreatorRole,
                nameof(_discordService.BannedRole) => _discordService.BannedRole,
                nameof(_discordService.ReadOnlyRole) => _discordService.ReadOnlyRole,
                nameof(_discordService.DonatorRole) => _discordService.DonatorRole,
                _ => throw new Exception("Invalid role name")
            };

            await member.RevokeRoleAsync(discordRole);
        }
    }
}