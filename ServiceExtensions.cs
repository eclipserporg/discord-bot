using app.Apis;
using app.Settings;
using Microsoft.OpenApi.Models;
using Refit;
using System.Net.Http.Headers;
using System.Text;

namespace app;

public static class ServiceExtensions
{
    public static IServiceCollection AddRefitServices(this IServiceCollection collection, ServerApiSettings serverApiSettings)
    {
        collection.AddRefitClient<IServerDiscordApi>().ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(serverApiSettings.Url);
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{serverApiSettings.Username}:{serverApiSettings.Password}")));
        });

        return collection;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection collection)
    {
        collection.AddSwaggerGen(c =>
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
                    Array.Empty<string>()
                }
            });
        });
        return collection;
    }
}
