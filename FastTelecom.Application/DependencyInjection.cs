using FastTelecom.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastTelecom.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<SessionStore>();

            services.AddTransient<AuthenticationService>();
            services.AddTransient<BundleService>();

            return services;
        }
    }
}