namespace app.Services
{
    public class RunnerService : BackgroundService
    {
        public RunnerService(DiscordService discordService, 
            LoginService loginService, 
            GuildJoinService guildJoinService)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Running...");
        }
    }
}
