using app.Apis;
using Refit;

namespace app
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRefitServices(this IServiceCollection collection)
        {
            collection.AddRefitClient<IServerDiscordApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:9696/api/"));

            return collection;
        }
    }
}
