using DiscordBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DiscordBot.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class FeedsController : ControllerBase
{
    private readonly DiscordService _discordService;
    private const int _maxMessageLength = 2000;
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

        if (message.Length > _maxMessageLength)
        {
            var messageParts = message.Split(" ");

            var currentMessage = string.Empty;

            foreach (var messagePart in messageParts)
            {
                if (currentMessage.Length + messagePart.Length > _maxMessageLength)
                {
                    await _discordService.Client.SendMessageAsync(discordChannel, currentMessage);
                    currentMessage = string.Empty;
                }

                currentMessage += messagePart + " ";
            }

            return;
        }


        await _discordService.Client.SendMessageAsync(discordChannel, message);
    }
}
