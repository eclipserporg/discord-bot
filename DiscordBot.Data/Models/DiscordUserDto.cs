namespace DiscordBot.Models;
#nullable disable
public class DiscordUserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string AvatarUrl { get; set; }
}
