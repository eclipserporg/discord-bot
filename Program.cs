using app.Services;
using app.Settings;
using app;
using app.Models;
using app.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Serilog.Events;
using Serilog;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.WebHost.UseUrls("http://*:7031");


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console() 
    .CreateLogger();

var serverApiSettings = new ServerApiSettings();
builder.Configuration.GetSection(nameof(ServerApiSettings)).Bind(serverApiSettings);



builder.Services.Configure<DiscordSettings>(options => builder.Configuration.GetSection(nameof(DiscordSettings)).Bind(options));
builder.Services.Configure<ServerApiSettings>(options => builder.Configuration.GetSection(nameof(ServerApiSettings)).Bind(options));
builder.Services.Configure<Credentials>(options => builder.Configuration.GetSection(nameof(Credentials)).Bind(options));

builder.Services.AddRefitServices(serverApiSettings);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DiscordService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddSingleton<RunnerService>();
builder.Services.AddSingleton<GuildJoinService>();
builder.Services.AddHostedService(provider => provider.GetService<RunnerService>());
builder.Services.AddAuthentication("BasicAuthentication")
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddSwaggerServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.Services.GetService<DiscordService>().Start();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (!context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress))
    {
        context.Response.StatusCode = 403;
        return;
    }
    await next.Invoke();
});

app.MapControllers();
app.Run();
