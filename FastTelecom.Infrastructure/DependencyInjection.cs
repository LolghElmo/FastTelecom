using FastTelecom.Domain.Interfaces;
using FastTelecom.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace FastTelecom.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            services.AddSingleton<ICryptoService, CryptoService>();

            static void ConfigureClient(HttpClient client)
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept",
                    "application/json, text/javascript, */*; q=0.01");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.Timeout = TimeSpan.FromSeconds(30);
            }
            var cookieContainer = new CookieContainer();
            HttpClientHandler MakeHandler() => new()
            {
                CookieContainer = cookieContainer,
                UseCookies      = true,
            };

            services.AddHttpClient<ITarasClient, TarasClient>(ConfigureClient)
                    .ConfigurePrimaryHttpMessageHandler(MakeHandler);

            services.AddHttpClient<IBundleClient, BundleClient>(ConfigureClient)
                    .ConfigurePrimaryHttpMessageHandler(MakeHandler);

            return services;
        }
    }
}
