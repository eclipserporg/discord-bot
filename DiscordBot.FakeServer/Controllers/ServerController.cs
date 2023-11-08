using DiscordBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.FakeServer.Controllers;

[ApiController]
[Authorize]
[Route("[action]")]
public class ServerController : ControllerBase
{
    [HttpPost("/remove-read-only")]
    public bool PostRemoveReadOnly(string channel, ulong senderId, string senderName, ulong targetId, string targetName)
    {
        return true;
    }

    [HttpPost("/read-only")]
    public bool PostReadOnly(string channel, ulong senderId, string senderName, ulong targetId, string targetName, string reason)
    {
        return true;
    }

    [HttpGet("/apps")]
    public string GetApps()
    {
        return "Currently no apps...";
    }

    [HttpGet("/account-status")]
    public string GetAccountStatus(ulong id)
    {
        return "[]";
    }

    [HttpPost("/announce")]
    public void PostAnnouncement(string text)
    {
    }

    [HttpPost("/save")]
    public bool PostSave()
    {
        return true;
    }

    [HttpGet("/ping")]
    public bool GetPing()
    {
        return true;
    }

    [HttpPost("/kick")]
    public bool PostKick(string channel, ulong senderId, string senderName, ulong targetId, string targetName, string reason)
    {
        return true;
    }

    [HttpPost("/ban")]
    public bool PostBan(string channel, ulong senderId, string senderName, ulong targetId, string targetName, string reason)
    {
        return true;
    }

    [HttpPost("/login")]
    public ResponsePairDto PostLogin(string name, ulong id, string username, string discriminator, string avatarurl, string password)
    {
        return new ResponsePairDto()
        {
            Message = "Success",
            Status = true
        };
    }
}