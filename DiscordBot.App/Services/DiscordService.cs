using DiscordBot.Commands;
using DiscordBot.Settings;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using Microsoft.Extensions.Options;
using Serilog;

namespace DiscordBot.Services;

public class DiscordService
{
    public DiscordClient Client { get; private set; }
    public DiscordGuild Guild { get; private set; }
    private readonly DiscordSettings _settings;
    private readonly ServerApiSettings _serverApiSettings;
    private Func<DiscordClient, GuildMemberAddedEventArgs, Task>? _guildMemberAddedHandler;

    // Fix this
    public DiscordRole MemberRole { get; private set; }
    public DiscordRole ReadOnlyRole { get; private set; }
    public DiscordRole BannedRole { get; private set; }
    public DiscordRole CreatorRole { get; private set; }
    public DiscordChannel HelpVerifyChannel { get; private set; }
    public DiscordChannel VerificationChannel { get; private set; }
    public DiscordChannel CommandsChannel { get; private set; }
    public DiscordChannel GeneralChannel { get; private set; }
    public DiscordChannel SurveilanceChannel { get; private set; }
    public DiscordChannel CheatChannel { get; private set; }
    public DiscordChannel VerifyLogsChannel { get; private set; }
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

    public void SetGuildMemberAddedHandler(Func<DiscordClient, GuildMemberAddedEventArgs, Task> handler)
    {
        _guildMemberAddedHandler = handler;
    }

    public async Task Start()
    {
        var builder = DiscordClientBuilder.CreateDefault(
            _settings.Token,
            DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.DirectMessages);

        builder.ConfigureServices(services =>
        {
            services.AddRefitServices(_serverApiSettings);
            services.AddSingleton(this);
            services.AddSingleton<RestartState>();
        });

        builder.UseCommands((sp, ext) =>
        {
            ext.ConfiguringCommands += async (_, e) =>
            {
                e.CommandTrees.Add(CommandBuilder.From<AuthenticationCommands>());
                e.CommandTrees.Add(CommandBuilder.From<VerifyCommand>());
                e.CommandTrees.Add(CommandBuilder.From<PingServerCommand>());
                e.CommandTrees.Add(CommandBuilder.From<RemoveReadOnlyCommand>());
                e.CommandTrees.Add(CommandBuilder.From<ReadOnlyCommand>());
                e.CommandTrees.Add(CommandBuilder.From<AppsCommand>());
                e.CommandTrees.Add(CommandBuilder.From<AnnCommand>());
                e.CommandTrees.Add(CommandBuilder.From<SaveCommand>());
                e.CommandTrees.Add(CommandBuilder.From<KickCommand>());
                e.CommandTrees.Add(CommandBuilder.From<BanCommand>());
                e.CommandTrees.Add(CommandBuilder.From<StopRestartCommand>());
                e.CommandTrees.Add(CommandBuilder.From<StartRestartCommand>());
                await Task.CompletedTask;
            };
        });

        builder.ConfigureEventHandlers(b =>
        {
            b.HandleGuildMemberAdded(async (client, e) =>
            {
                if (_guildMemberAddedHandler != null)
                    await _guildMemberAddedHandler(client, e);
            });
        });

        Client = builder.Build();

        await Client.ConnectAsync();

        Guild = await Client.GetGuildAsync(_settings.Guild);

        foreach (var role in Guild.Roles.Values)
        {
            if (role.Id == _settings.Roles.Member)
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

        await SetPresence(DiscordActivityType.Playing, "ECLIPSE Roleplay!");
    }

    public async Task SetPresence(DiscordActivityType type, string message)
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
}
