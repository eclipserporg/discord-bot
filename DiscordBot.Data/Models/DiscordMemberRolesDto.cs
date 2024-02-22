using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Data.Models;

public class DiscordMemberRolesDto
{
    /// <summary>
    /// A collection of VIP Discord role ids.
    /// </summary>
    public HashSet<ulong> RolesVIP { get; set; } = new();

    /// <summary>
    /// A collection of VIP Discord member ids and their role ids.
    /// </summary>
    public Dictionary<ulong, ulong> MemberRolesVIP { get; set; } = new();

    /// <summary>
    /// A collection of donator Discord role ids.
    /// </summary>
    public HashSet<ulong> RolesDonator { get; set; } = new();

    /// <summary>
    /// A collection of donator Discord member ids and their role ids.
    /// </summary>
    public Dictionary<ulong, ulong> MemberRolesDonator { get; set; } = new();

    /// <summary>
    /// A collection of content creator Discord member ids.
    /// </summary
    public HashSet<ulong> ContentCreatorMemberIds { get; set; } = new();
}
