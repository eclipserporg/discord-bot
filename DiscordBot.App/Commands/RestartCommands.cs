using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Serilog;

namespace DiscordBot.Commands;

[Command("stoprestart")]
[Description("Used for stopping inprogress restart!")]
public class StopRestartCommand
{
    private readonly IServerDiscordApi _serverDiscordApi;
    private readonly DiscordService _discordService;
    private readonly RestartState _state;

    public StopRestartCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService, RestartState state)
    {
        _serverDiscordApi = serverDiscordApi;
        _discordService = discordService;
        _state = state;
    }

    [Command("stoprestart")]
    public async Task ExecuteAsync(SlashCommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        if (!_state.InProgress)
        {
            await ctx.RespondAsync("No restart in progress");
            return;
        }

        _state.CancellationTokenSource.Cancel();
        await ctx.RespondAsync("Restart cancelled");
    }
}

[Command("startrestart")]
[Description("Used starting a restart. Time is in minutes!")]
public class StartRestartCommand
{
    private readonly IServerDiscordApi _serverDiscordApi;
    private readonly DiscordService _discordService;
    private readonly RestartState _state;
    private const int MinimumRestartMinutes = 5;

    public StartRestartCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService, RestartState state)
    {
        _serverDiscordApi = serverDiscordApi;
        _discordService = discordService;
        _state = state;
    }

    [Command("startrestart")]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("minutes")] [Description("restart in minutes")] long minutes)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        if (_state.InProgress)
        {
            await ctx.RespondAsync("Restart already in progress");
            return;
        }

        if (minutes < MinimumRestartMinutes)
        {
            await ctx.RespondAsync($"Minimum restart time is {MinimumRestartMinutes} minutes");
            return;
        }

        _state.Minutes = minutes;
        _state.Initiator = ctx.User;
        _state.InProgress = true;
        _state.CancellationTokenSource = new CancellationTokenSource();

        await ctx.RespondAsync($"Starting server restart in {minutes} minutes");
        await RestartLoop(_state.CancellationTokenSource.Token);
    }

    private async Task RestartLoop(CancellationToken token)
    {
        try
        {
            while (_state.Minutes >= 0)
            {
                if (_state.Minutes == 0)
                {
                    await _serverDiscordApi.PostAnnouncement("Server will restart now");
                    Log.Information("Saving server...");
                    await _serverDiscordApi.PostSave();
                    await _discordService.CommandsChannel.SendMessageAsync($"{_state.Initiator!.Mention} Server has been saved. Continue with restart.");
                    _state.InProgress = false;
                    return;
                }
                else
                {
                    await _serverDiscordApi.PostAnnouncement($"Server will restart in {_state.Minutes} minutes");
                    Log.Information($"Server will restart in {_state.Minutes} minutes");
                    await Task.Delay(TimeSpan.FromMinutes(1), token);
                    _state.Minutes--;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while restarting server");
            await _discordService.CommandsChannel.SendMessageAsync($"Error while restarting server: {e.Message}");
            _state.InProgress = false;
        }
    }
}

/// <summary>Shared mutable state between stoprestart and startrestart commands.</summary>
public class RestartState
{
    public long Minutes { get; set; }
    public bool InProgress { get; set; }
    public DiscordUser? Initiator { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
}
