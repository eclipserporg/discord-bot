using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Data.Models;

public class DiscordMemberRolesDto
{
    public HashSet<ulong> RolesVIP { get; set; }
    public Dictionary<ulong, ulong> MemberRolesVIP { get; set; }
}
