using DiscordBot.Apis;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace DiscordBot.Services;

public class LoginService
{
    private readonly DiscordService _discordService;
    private readonly IServerDiscordApi _serverDiscordApi;

    public LoginService(DiscordService discordService, IServerDiscordApi serverDiscordApi)
    {
        _discordService = discordService;
        _serverDiscordApi = serverDiscordApi;

        discordService.Client.MessageCreated += OnMessageCreated;
    }

    private async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot)
            return;

        if (e.Channel.Type == ChannelType.Private)
        {
            await OnPrivateMessage(e);
        }
    }

    private async Task HandleLoginCommand(MessageCreateEventArgs e, string name, string password)
    {
        var discordMember = await _discordService.Guild.GetMemberAsync(e.Author.Id);

        if (discordMember == null)
        {
            await e.Channel.SendMessageAsync("You are not a member of our Discord server!");
            return;
        }

        var response = await _serverDiscordApi.PostLogin(name, discordMember.Id, discordMember.Username, discordMember.Discriminator, discordMember.AvatarUrl, password);

        if(response.Status)
        {
            await discordMember.GrantRoleAsync(_discordService.MemberRole);
        }

        await e.Message.RespondAsync(response.Message);
    }

    private async Task OnPrivateMessage(MessageCreateEventArgs e)
    {
        var message = e.Message;
        var content = message.Content;

        var trimmed = content.TrimStart();

        if (trimmed.StartsWith("!login"))
        {
            var input = trimmed.Remove(0, 7);
            var index = input.IndexOf(' ');

            if (index == -1)
            {
                await e.Message.RespondAsync("Incorrect format! Write **!login USERNAME PASSWORD**");
                return;
            }

            var accountName = input[..index];
            var password = input.Substring(index + 1, input.Length - index - 1);

            await HandleLoginCommand(e, accountName, password);
        }
        else
        {
            await e.Message.RespondAsync("Incorrect format! Write **!login USERNAME PASSWORD**");
        }
    }
}
