using System.ComponentModel;
using DiscordBot.Apis;
using DiscordBot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Entities;
using Serilog;

namespace DiscordBot.Commands;

public class StopRestartCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService, RestartState state)
{
    [Command("stoprestart")]
    [Description("Used for stopping inprogress restart!")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx)
    {
        if (ctx.Channel != discordService.CommandsChannel)
            return;

        if (!state.InProgress)
        {
            await ctx.RespondAsync("No restart in progress");
            return;
        }

        state.CancellationTokenSource.Cancel();
        await ctx.RespondAsync("Restart cancelled");
    }
}

public class StartRestartCommand(IServerDiscordApi serverDiscordApi, DiscordService discordService, RestartState state)
{
    private const int MinimumRestartMinutes = 5;

    [Command("startrestart")]
    [Description("Used starting a restart. Time is in minutes!")]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public async Task ExecuteAsync(SlashCommandContext ctx, [Parameter("minutes")] [Description("restart in minutes")] long minutes)
    {
        if (ctx.Channel != discordService.CommandsChannel)
            return;

        if (state.InProgress)
        {
            await ctx.RespondAsync("Restart already in progress");
            return;
        }

        if (minutes < MinimumRestartMinutes)
        {
            await ctx.RespondAsync($"Minimum restart time is {MinimumRestartMinutes} minutes");
            return;
        }

        state.Minutes = minutes;
        state.Initiator = ctx.User;
        state.InProgress = true;
        state.CancellationTokenSource = new CancellationTokenSource();

        await ctx.RespondAsync($"Starting server restart in {minutes} minutes");
        await RestartLoop(state.CancellationTokenSource.Token);
    }

    private async Task RestartLoop(CancellationToken token)
    {
        try
        {
            while (state.Minutes >= 0)
            {
                if (state.Minutes == 0)
                {
                    await serverDiscordApi.PostAnnouncement("Server will restart now");
                    Log.Information("Saving server...");
                    await serverDiscordApi.PostSave();
                    await discordService.CommandsChannel.SendMessageAsync($"{state.Initiator!.Mention} Server has been saved. Continue with restart.");
                    state.InProgress = false;
                    return;
                }
                else
                {
                    await serverDiscordApi.PostAnnouncement($"Server will restart in {state.Minutes} minutes");
                    Log.Information($"Server will restart in {state.Minutes} minutes");
                    await Task.Delay(TimeSpan.FromMinutes(1), token);
                    state.Minutes--;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while restarting server");
            await discordService.CommandsChannel.SendMessageAsync($"Error while restarting server: {e.Message}");
            state.InProgress = false;
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
