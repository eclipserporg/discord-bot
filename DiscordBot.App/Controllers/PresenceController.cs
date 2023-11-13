using DiscordBot.Services;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DiscordBot.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class PresenceController : ControllerBase
{
    private readonly DiscordService _discordService;
    // create a new variable in the database, instead of 400 this must be fetched from said database
    private int allTimeHighPlayerCount = 400;

    public PresenceController(DiscordService discordService)
    {
        _discordService = discordService;
    }

    private async Task SendMessageToGeneralChannel(string message)
    {
        DiscordChannel generalChannel = _discordService.GeneralChannel;
        
        if (generalChannel != null)
        {
            await generalChannel.SendMessageAsync(message);
        }
    }

    [HttpPost(Name = "players")]
    public async Task PostPlayers(int playerCount)
    {
        Log.Information("Post players");
        
        if(playerCount > allTimeHighPlayerCount)
        {
            await _discordService.Client.UpdateStatusAsync(new DiscordActivity($"with a all time high of {allTimeHighPlayerCount} players!", ActivityType.Custom, ActivityProperties.None, "Roleplaying"));
            //create post to replace allTimePlayerCount : allTimeHighPlayerCount = playerCount  
            SendMessageToGeneralChannel($"We hit a all time peak of {allTimeHighPlayerCount} players!!!");   
        }
        else
        {
            await _discordService.Client.UpdateStatusAsync(new DiscordActivity($"with {playerCount} players!", ActivityType.Custom, ActivityProperties.None, "Roleplaying"));        
        }
    }
}
