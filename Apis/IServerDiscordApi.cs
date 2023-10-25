using app.Models;
using Refit;

namespace app.Apis;

public interface IServerDiscordApi
{
    [Post("/remove-read-only")]
    Task<bool> PostRemoveReadOnly(string channel, ulong senderId, string senderName, ulong targetId, string targetName);

    [Post("/read-only")]
    Task<bool> PostReadOnly(string channel, ulong senderId, string senderName, ulong targetId, string targetName, string reason);

    [Get("/apps")]
    Task<string> GetApps();

    [Get("/account-status")]
    Task<string> GetAccountStatus(ulong id);

    [Post("/announce")]
    Task PostAnnouncement(string text);

    [Post("/save")]
    Task<bool> PostSave();

    [Post("/kick")]
    Task<bool> PostKick(string channel, ulong senderId, string senderName, ulong targetId, string targetName, string reason);

    [Post("/ban")]
    Task<bool> PostBan(string channel, ulong senderId, string senderName, ulong targetId, string targetName, string reason);

    [Post("/login")]
    Task<ResponsePairDto> PostLogin(string name, ulong id, string username, string discriminator, string avatarurl, string password);
}
