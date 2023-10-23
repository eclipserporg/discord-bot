namespace app.Settings
{
    public class Roles
    {
        public ulong Member { get; set; }
        public ulong Banned { get; set; }
        public ulong Donator { get; set; }
        public ulong Creator { get; set; }
        public ulong ReadOnly { get; set; }
    }

    public class Channels
    {
        public ulong Verification { get; set; }
        public ulong HelpVerify { get; set; }
        public ulong Commands { get; set; }
        public ulong General { get; set; }
        public ulong Surveilance { get; set; }
        public ulong Cheat { get; set; }
        public ulong VerifyLogs { get; set; }
        public ulong LinkedAccounts { get; set; }
        public ulong LinkedAccountsCompact { get; set; }
        public ulong DiscordBotLogs { get; set; }
        public ulong WeazelFeed { get; set; }
        public ulong QuizUpdates { get; set; }
        public ulong AccountCreation { get; set; }
        public ulong BanNotifications { get; set; }
    }

    public class DiscordSettings
    {
        public string Token { get; set; }
        public  ulong Guild { get; set; }
        public Roles Roles { get; set; }
        public Channels Channels { get; set; }
    }
}
