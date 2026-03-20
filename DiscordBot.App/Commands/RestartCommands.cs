using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Serilog;

namespace DiscordBot.Commands;

public class RestartCommands
{
    private readonly IServerDiscordApi _serverDiscordApi;
    private readonly DiscordService _discordService;
    private readonly int _minimumRestartMinutes = 5;
    private CancellationTokenSource _restartCancellationTokenSource;

    private long _restartMinutes = 0;
    private bool _restartInProgress = false;
    private DiscordUser _restartInitiator;

    public RestartCommands(IServerDiscordApi serverDiscordApi, DiscordService discordService)
    {
        _serverDiscordApi = serverDiscordApi;
        _discordService = discordService;
        _restartCancellationTokenSource = new CancellationTokenSource();
    }

    private async Task RestartLoop(CancellationToken token)
    {
        try
        {
            while (_restartMinutes >= 0)
            {
                if (_restartMinutes == 0)
                {
                    await _serverDiscordApi.PostAnnouncement("Server will restart now");
                    Log.Information("Saving server...");
                    await _serverDiscordApi.PostSave();
                    await _discordService.CommandsChannel.SendMessageAsync($"{_restartInitiator.Mention} Server has been saved. Continue with restart.");
                    _restartInProgress = false;
                    return;
                }
                else
                {
                    await _serverDiscordApi.PostAnnouncement($"Server will restart in {_restartMinutes} minutes");
                    Log.Information($"Server will restart in {_restartMinutes} minutes");
                    await Task.Delay(TimeSpan.FromMinutes(1), token);
                    _restartMinutes--;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while restarting server");
            await _discordService.CommandsChannel.SendMessageAsync($"Error while restarting server: {e.Message}");
            _restartInProgress = false;
            return;
        }
    }

    [Command("stoprestart")]
    [Description("Used for stopping inprogress restart!")]
    public async Task StopRestartCommand(SlashCommandContext ctx)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        if (!_restartInProgress)
        {
            await ctx.RespondAsync("No restart in progress");
            return;
        }

        _restartCancellationTokenSource.Cancel();
        await ctx.RespondAsync("Restart cancelled");
    }

    [Command("startrestart")]
    [Description("Used starting a restart. Time is in minutes!")]
    public async Task StartRestartCommand(SlashCommandContext ctx, [Parameter("minutes")] [Description("restart in minutes")] long minutes)
    {
        if (ctx.Channel != _discordService.CommandsChannel)
            return;

        if (_restartInProgress)
        {
            await ctx.RespondAsync("Restart already in progress");
            return;
        }

        if (minutes < _minimumRestartMinutes)
        {
            await ctx.RespondAsync($"Minimum restart time is {_minimumRestartMinutes} minutes");
            return;
        }

        _restartMinutes = minutes;
        _restartInitiator = ctx.User;
        _restartInProgress = true;
        await ctx.RespondAsync($"Starting server restart in {minutes} minutes");
        await RestartLoop(_restartCancellationTokenSource.Token);
    }
}
