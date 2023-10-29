using app.Apis;
using DSharpPlus.Entities;
using Serilog;

namespace app.Services;

public class RunnerService : BackgroundService
{
    private readonly DiscordService _discordService;
    private readonly LoginService _loginService;
    private readonly GuildJoinService _guildJoinService;
    private readonly IServerDiscordApi _serverApi;


    public RunnerService(DiscordService discordService, LoginService loginService, GuildJoinService guildJoinService, IServerDiscordApi serverApi)
    {
        _discordService = discordService;
        _loginService = loginService;
        _guildJoinService = guildJoinService;
        _serverApi = serverApi;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var status = await _serverApi.GetPing();

                if(!status)
                {
                    throw new Exception("Server is down");
                }
            }
            catch(Exception e)
            {
                await _discordService.SetPresence(ActivityType.Watching, "server status: DOWN !");
                Log.Error(e, "RunnerService");
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        Console.WriteLine("Running...");
    }
}
