using DiscordBot.Settings;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DiscordBot.Commands;
using Serilog;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Services;

public class DiscordService
{
    public DiscordClient Client { get; private set; }
    public DiscordGuild Guild { get; private set; }
    private readonly DiscordSettings _settings;
    private readonly ServerApiSettings _serverApiSettings;

    // Fix this
    public DiscordRole MemberRole { get; private set; }
    public DiscordRole ReadOnlyRole { get; private set; }
    public DiscordRole BannedRole { get; private set; }
    public DiscordRole DonatorRole { get; private set; }
    public DiscordRole CreatorRole { get; private set; }
    public DiscordChannel HelpVerifyChannel { get; private set; }
    public DiscordChannel VerificationChannel { get; private set; }
    public DiscordChannel CommandsChannel { get; private set; }
    public DiscordChannel GeneralChannel { get; private set; }
    public DiscordChannel SurveilanceChannel { get; private set; }
    public DiscordChannel CheatChannel { get; private set; }
    public DiscordChannel VerifyLogsChannel { get; private  set; }
    public DiscordChannel LinkedAccountsChannel { get; private set; }
    public DiscordChannel LinkedAccountsCompactChannel { get; private set; }
    public DiscordChannel DiscordBotLogsChannel { get; private set; }
    public DiscordChannel WeazelFeedChannel { get; private set; }
    public DiscordChannel QuizUpdatesChannel { get; private set; }
    public DiscordChannel AccountCreationChannel { get; private set; }
    public DiscordChannel BanNotificationsChannel { get; private set; }

    public DiscordService(IOptions<DiscordSettings> discordSettings, IOptions<ServerApiSettings> settings)
    {
        _serverApiSettings = settings.Value;
        _settings = discordSettings.Value;
    }

    public async Task Start()
    {
        var configuration = new DiscordConfiguration
        {
            TokenType = TokenType.Bot,
            Token = _settings.Token,
            ReconnectIndefinitely = true,
            AutoReconnect = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.DirectMessages
        };

        Client = new DiscordClient(configuration);

        var slashConfig = new SlashCommandsConfiguration()
        {
            Services = new ServiceCollection()
                .AddRefitServices(_serverApiSettings)
                .AddSingleton(this)
                .BuildServiceProvider()
        };

        var slashCommands = Client.UseSlashCommands(slashConfig);
        slashCommands.RegisterCommands<AuthenticationCommands>();
        slashCommands.RegisterCommands<GeneralCommands>();
        slashCommands.RegisterCommands<RestartCommands>();
        
        await Client.InitializeAsync();
        await Client.ConnectAsync();

        Guild = await Client.GetGuildAsync(_settings.Guild);

        foreach (var role in Guild.Roles.Values)
        {
            if (role.Id == _settings.Roles.Donator)
                DonatorRole = role;
            else if (role.Id == _settings.Roles.Member)
                MemberRole = role;
            else if (role.Id == _settings.Roles.Banned)
                BannedRole = role;
            else if (role.Id == _settings.Roles.Creator)
                CreatorRole = role;
            else if (role.Id == _settings.Roles.ReadOnly)
                ReadOnlyRole = role;
        }

        foreach (var channel in Guild.Channels.Values)
        {
            if (channel.Id == _settings.Channels.General)
                GeneralChannel = channel;
            else if (channel.Id == _settings.Channels.Commands)
                CommandsChannel = channel;
            else if (channel.Id == _settings.Channels.Verification)
                VerificationChannel = channel;
            else if (channel.Id == _settings.Channels.HelpVerify)
                HelpVerifyChannel = channel;
            else if (channel.Id == _settings.Channels.VerifyLogs)
                VerifyLogsChannel = channel;
            else if (channel.Id == _settings.Channels.Surveilance)
                SurveilanceChannel = channel;
            else if (channel.Id == _settings.Channels.Cheat)
                CheatChannel = channel;
            else if (channel.Id == _settings.Channels.LinkedAccounts)
                LinkedAccountsChannel = channel;
            else if (channel.Id == _settings.Channels.LinkedAccountsCompact)
                LinkedAccountsCompactChannel = channel;
            else if (channel.Id == _settings.Channels.DiscordBotLogs)
                DiscordBotLogsChannel = channel;
            else if (channel.Id == _settings.Channels.WeazelFeed)
                WeazelFeedChannel = channel;
            else if (channel.Id == _settings.Channels.QuizUpdates)
                QuizUpdatesChannel = channel;
            else if (channel.Id == _settings.Channels.AccountCreation)
                AccountCreationChannel = channel;
            else if (channel.Id == _settings.Channels.BanNotifications)
                BanNotificationsChannel = channel;
        }

        Client.ClientErrored += OnClientError;
        Client.SocketErrored += OnSocketError;
        //commands.CommandErrored += OnCommandError;

        await SetPresence(ActivityType.Playing, "ECLIPSE Roleplay!");
    }

    public async Task SetPresence(ActivityType type, string message)
    {
        try
        {
            await Client.UpdateStatusAsync(new DiscordActivity(message, type));
        }
        catch
        {
            Log.Error("Failed to set presence");
        }
    }

    private Task OnClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        Log.Error(e.Exception, "OnClientError");
        Log.Error(e.ToString());
        return Task.CompletedTask;
    }

    private Task OnCommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        Log.Error(e.Exception, "OnCommandError");
        Log.Error(e.ToString());
        return Task.CompletedTask;
    }
    private Task OnSocketError(DiscordClient sender, SocketErrorEventArgs e)
    {
        Log.Error(e.Exception, "OnSocketError");
        Log.Error(e.ToString());
        return Task.CompletedTask;
    }
}
