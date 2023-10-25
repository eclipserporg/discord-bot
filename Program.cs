using app.Services;
using app.Settings;
using app;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using app.Models;
using Microsoft.Extensions.Configuration;
using app.Middlewares;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var serverApiSettings = new ServerApiSettings();
builder.Configuration.GetSection(nameof(ServerApiSettings)).Bind(serverApiSettings);

builder.Services.Configure<DiscordSettings>(options => builder.Configuration.GetSection(nameof(DiscordSettings)).Bind(options));
builder.Services.Configure<ServerApiSettings>(options => builder.Configuration.GetSection(nameof(ServerApiSettings)).Bind(options));
builder.Services.Configure<Credentials>(options => builder.Configuration.GetSection(nameof(Credentials)).Bind(options));

builder.Services.AddRefitServices(serverApiSettings);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DiscordService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddSingleton<RunnerService>();
builder.Services.AddSingleton<GuildJoinService>();
builder.Services.AddHostedService(provider => provider.GetService<RunnerService>());
builder.Services.AddAuthentication("BasicAuthentication")
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
            },
            new string[] { }
        }
    });
});


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

app.MapControllers();
app.Run();
