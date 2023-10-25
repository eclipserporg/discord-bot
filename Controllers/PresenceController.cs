using app.Services;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]/[action]")]
    public class PresenceController : ControllerBase
    {
        private readonly DiscordService _discordService;

        public PresenceController(DiscordService discordService)
        {
            _discordService = discordService;
        }

        [HttpPost(Name = "players")]
        public void PostPlayers(int playerCount)
        {
            _discordService.Client.UpdateStatusAsync(new DiscordActivity($"with {playerCount} players!", ActivityType.Playing));
        }
    }
}