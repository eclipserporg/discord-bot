using app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace app.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]/[action]")]
    public class FeedsController : ControllerBase
    {
        private readonly DiscordService _discordService;
        public FeedsController(DiscordService discordService)
        {
            _discordService = discordService;
        }

        [HttpPost(Name = "send")]
        public async Task PostSend(string channel, string message)
        {
            Log.Information("Post send");
            var discordChannel = channel switch
            {
                nameof (_discordService.HelpVerifyChannel) => _discordService.HelpVerifyChannel,
                nameof (_discordService.VerificationChannel) => _discordService.VerificationChannel,
                nameof (_discordService.CommandsChannel) => _discordService.CommandsChannel,
                nameof (_discordService.GeneralChannel) => _discordService.GeneralChannel,
                nameof (_discordService.SurveilanceChannel) => _discordService.SurveilanceChannel,
                nameof (_discordService.CheatChannel) => _discordService.CheatChannel,
                nameof (_discordService.VerifyLogsChannel) => _discordService.VerifyLogsChannel,
                nameof (_discordService.LinkedAccountsChannel) => _discordService.LinkedAccountsChannel,
                nameof (_discordService.LinkedAccountsCompactChannel) => _discordService.LinkedAccountsCompactChannel,
                nameof (_discordService.DiscordBotLogsChannel) => _discordService.DiscordBotLogsChannel,
                nameof (_discordService.WeazelFeedChannel) => _discordService.WeazelFeedChannel,
                nameof (_discordService.QuizUpdatesChannel) => _discordService.QuizUpdatesChannel,
                nameof (_discordService.AccountCreationChannel) => _discordService.AccountCreationChannel,
                nameof (_discordService.BanNotificationsChannel) => _discordService.BanNotificationsChannel,
                _ => throw new Exception("Invalid channel name")
            };

            await _discordService.Client.SendMessageAsync(discordChannel, message);
        }
    }
}
