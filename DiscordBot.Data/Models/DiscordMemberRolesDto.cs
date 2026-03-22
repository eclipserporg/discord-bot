namespace DiscordBot.Data.Models;

public class DiscordMemberRolesDto
{
    /// <summary>
    /// A collection of VIP role ids.
    /// </summary>
    public HashSet<ulong> RoleIdsVIP { get; set; } = new();

    /// <summary>
    /// A collection of donator role ids.
    /// </summary>
    public HashSet<ulong> RoleIdsDonator { get; set; } = new();

    /// <summary>
    /// A collection of member and their VIP role ids.
    /// </summary
    public Dictionary<ulong, ulong> MemberRoleIdsVIP { get; set; } = new();

    /// <summary>
    /// A collection of member and their donator role ids.
    /// </summary>
    public Dictionary<ulong, ulong> MemberRoleIdsDonator { get; set; } = new();

    /// <summary>
    /// A collection of content creator member ids.
    /// </summary
    public HashSet<ulong> ContentCreatorMemberIds { get; set; } = new();
}
