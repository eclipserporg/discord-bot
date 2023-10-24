using app.Apis;
using app.Settings;
using Refit;
using System.Net.Http.Headers;
using System.Text;

namespace app
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRefitServices(this IServiceCollection collection, ServerApiSettings serverApiSettings)
        {
            collection.AddRefitClient<IServerDiscordApi>().ConfigureHttpClient( c =>
            {
                c.BaseAddress = new Uri(serverApiSettings.Url);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{serverApiSettings.Username}:{serverApiSettings.Password}")));
            });

            return collection;
        }
    }
}
